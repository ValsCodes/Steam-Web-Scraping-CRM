export function encode(value: string): string {
  if (value === null || value === undefined) {
    return '';
  }

  return encodeURIComponent(value);
}
