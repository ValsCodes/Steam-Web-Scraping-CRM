export function getListingUrl(
  internalGameId: number | null | undefined,
  listingName: string | null | undefined,
): string {
  if (internalGameId === null || internalGameId === undefined || internalGameId <= 0) {
    return '';
  }

  const name = listingName?.trim();

  if (!name) {
    return '';
  }

  return `https://steamcommunity.com/market/listings/${internalGameId}/${encodeURIComponent(name)}`;
}
