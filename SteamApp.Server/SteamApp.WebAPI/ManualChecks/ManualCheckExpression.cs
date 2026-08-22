using SteamApp.Application.DTOs.ManualCheck;
using SteamApp.Domain.Enums;

namespace SteamApp.WebAPI.ManualChecks;

public static class ManualCheckExpression
{
    public const int MaxGroups = 25;

    public static string? Validate(IReadOnlyList<ManualCheckCriterionDto> criteria)
    {
        if (criteria.Count == 0)
        {
            return null;
        }

        if (criteria[0].ConditionOperatorId.HasValue)
        {
            return "The first criterion cannot have a condition operator.";
        }

        for (var index = 1; index < criteria.Count; index++)
        {
            var conditionOperatorId = criteria[index].ConditionOperatorId;
            if (!conditionOperatorId.HasValue ||
                !Enum.IsDefined((ManualCheckConditionOperatorEnum)conditionOperatorId.Value))
            {
                return $"Criterion #{index + 1} requires a supported condition operator.";
            }
        }

        return ValidateGrouping(criteria);
    }

    public static string? ValidateGrouping(IReadOnlyList<ManualCheckCriterionDto> criteria)
    {
        var depth = 0;
        var totalGroups = 0;

        for (var index = 0; index < criteria.Count; index++)
        {
            var criterion = criteria[index];
            if (criterion.OpenGroupCount is < 0 or > MaxGroups ||
                criterion.CloseGroupCount is < 0 or > MaxGroups)
            {
                return $"Criterion #{index + 1} group counts must be between 0 and {MaxGroups}.";
            }

            totalGroups += criterion.OpenGroupCount;
            if (totalGroups > MaxGroups)
            {
                return $"A preset cannot contain more than {MaxGroups} explicit groups.";
            }

            depth += criterion.OpenGroupCount;
            if (depth > MaxGroups)
            {
                return $"Criterion groups cannot be nested more than {MaxGroups} levels deep.";
            }

            if (criterion.CloseGroupCount > depth)
            {
                return $"Criterion #{index + 1} closes a group that was not opened.";
            }

            depth -= criterion.CloseGroupCount;
        }

        return depth == 0
            ? null
            : $"The criteria expression contains {depth} unclosed group(s).";
    }

    public static bool Evaluate(
        IReadOnlyList<bool> criterionMatches,
        IReadOnlyList<ManualCheckCriterionDto> criteria)
    {
        if (criterionMatches.Count != criteria.Count)
        {
            throw new ArgumentException("Criterion matches must have the same count as criteria.", nameof(criterionMatches));
        }

        if (criteria.Count == 0)
        {
            return false;
        }

        var validationError = Validate(criteria);
        if (validationError is not null)
        {
            throw new InvalidOperationException(validationError);
        }

        var frames = new Stack<EvaluationFrame>();
        frames.Push(new EvaluationFrame(null));

        for (var index = 0; index < criteria.Count; index++)
        {
            var criterion = criteria[index];
            var incomingOperator = GetOperator(criterion, index);

            for (var groupIndex = 0; groupIndex < criterion.OpenGroupCount; groupIndex++)
            {
                frames.Push(new EvaluationFrame(groupIndex == 0 ? incomingOperator : null));
            }

            frames.Peek().Append(
                criterionMatches[index],
                criterion.OpenGroupCount > 0 ? null : incomingOperator,
                index);

            for (var groupIndex = 0; groupIndex < criterion.CloseGroupCount; groupIndex++)
            {
                var completed = frames.Pop();
                frames.Peek().Append(completed.Value, completed.OperatorToParent, index);
            }
        }

        return frames.Pop().Value;
    }

    private static ManualCheckConditionOperatorEnum? GetOperator(
        ManualCheckCriterionDto criterion,
        int index)
    {
        if (!criterion.ConditionOperatorId.HasValue)
        {
            return null;
        }

        var conditionOperator = (ManualCheckConditionOperatorEnum)criterion.ConditionOperatorId.Value;
        if (!Enum.IsDefined(conditionOperator))
        {
            throw new InvalidOperationException($"Criterion #{index + 1} has an unsupported condition operator.");
        }

        return conditionOperator;
    }

    private static bool Apply(
        bool accumulator,
        bool operand,
        ManualCheckConditionOperatorEnum conditionOperator)
    {
        return conditionOperator switch
        {
            ManualCheckConditionOperatorEnum.And => accumulator && operand,
            ManualCheckConditionOperatorEnum.Or => accumulator || operand,
            ManualCheckConditionOperatorEnum.AndNot => accumulator && !operand,
            ManualCheckConditionOperatorEnum.OrNot => accumulator || !operand,
            ManualCheckConditionOperatorEnum.Xor => accumulator != operand,
            ManualCheckConditionOperatorEnum.Nand => !(accumulator && operand),
            ManualCheckConditionOperatorEnum.Nor => !(accumulator || operand),
            _ => throw new ArgumentOutOfRangeException(nameof(conditionOperator))
        };
    }

    private sealed class EvaluationFrame(ManualCheckConditionOperatorEnum? operatorToParent)
    {
        private bool hasValue;
        private bool value;

        public ManualCheckConditionOperatorEnum? OperatorToParent { get; } = operatorToParent;

        public bool Value => hasValue
            ? value
            : throw new InvalidOperationException("Criterion groups cannot be empty.");

        public void Append(
            bool operand,
            ManualCheckConditionOperatorEnum? conditionOperator,
            int criterionIndex)
        {
            if (!hasValue)
            {
                if (conditionOperator.HasValue)
                {
                    throw new InvalidOperationException(
                        $"Criterion #{criterionIndex + 1} cannot have an operator at the start of a group.");
                }

                value = operand;
                hasValue = true;
                return;
            }

            if (!conditionOperator.HasValue)
            {
                throw new InvalidOperationException(
                    $"Criterion #{criterionIndex + 1} has no condition operator.");
            }

            value = Apply(value, operand, conditionOperator.Value);
        }
    }
}
