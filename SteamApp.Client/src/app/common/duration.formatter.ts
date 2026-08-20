export function formatDuration(durationMilliseconds: number | null | undefined): string {
  if (durationMilliseconds === null || durationMilliseconds === undefined) {
    return 'Not recorded';
  }

  const milliseconds = Math.max(0, Math.round(durationMilliseconds));
  if (milliseconds < 1000) {
    return `${milliseconds} ms`;
  }

  const totalSeconds = Math.floor(milliseconds / 1000);
  const seconds = totalSeconds % 60;
  const totalMinutes = Math.floor(totalSeconds / 60);
  if (totalMinutes < 1) {
    return `${(milliseconds / 1000).toFixed(1)} s`;
  }

  const minutes = totalMinutes % 60;
  const hours = Math.floor(totalMinutes / 60);
  if (hours < 1) {
    return `${minutes}m ${seconds.toString().padStart(2, '0')}s`;
  }

  return `${hours}h ${minutes.toString().padStart(2, '0')}m ${seconds.toString().padStart(2, '0')}s`;
}
