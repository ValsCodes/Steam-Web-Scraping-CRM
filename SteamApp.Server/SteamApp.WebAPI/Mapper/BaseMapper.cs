namespace SteamApp.WebAPI.Mapper;

public static class BaseMapper
{
    public static TDto ToDto<TEntity, TDto>(this TEntity entity)
          where TEntity : class
          where TDto : class, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        var dto = new TDto();

        // Look for Id + Name in both
        var entityType = typeof(TEntity);
        var dtoType = typeof(TDto);

        var idSource = entityType.GetProperty("Id");
        var nameSource = entityType.GetProperty("Name");

        var idDest = dtoType.GetProperty("Id");
        var nameDest = dtoType.GetProperty("Name");

        if (idSource != null && idDest != null)
            idDest.SetValue(dto, idSource.GetValue(entity));

        if (nameSource != null && nameDest != null)
            nameDest.SetValue(dto, nameSource.GetValue(entity));

        return dto;
    }

    public static TEntity ToEntity<TDto, TEntity>(this TDto dto, bool includeId = false)
        where TDto : class
        where TEntity : class, new()
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var entity = new TEntity();

        var dtoType = typeof(TDto);
        var entityType = typeof(TEntity);

        var nameSource = dtoType.GetProperty("Name");
        var nameDest = entityType.GetProperty("Name");

        if (nameSource != null && nameDest != null)
            nameDest.SetValue(entity, nameSource.GetValue(dto));

        if (includeId)
        {
            var idSource = dtoType.GetProperty("Id");
            var idDest = entityType.GetProperty("Id");
            if (idSource != null && idDest != null)
                idDest.SetValue(entity, idSource.GetValue(dto));
        }

        return entity;
    }
}
