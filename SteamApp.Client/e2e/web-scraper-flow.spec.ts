import { expect, test } from '@playwright/test';

import { mockSteamApi, signIn } from './support/mock-api';

test('runs a web scrape and displays returned listings', async ({ page }) => {
  await mockSteamApi(page);
  await signIn(page);

  await page.getByRole('button', { name: 'Scraping' }).hover();
  await page.getByRole('menuitem', { name: 'Web Scraper' }).click();

  await expect(page.getByRole('heading', { name: 'Web Scraper' })).toBeVisible();

  await page.locator('#web-scraper-game-id').selectOption({ label: 'Alpha Game' });
  await page.locator('#web-scraper-scraping-mode-id').selectOption({ label: 'Batch' });
  await page.locator('#web-scraper-game-url-id').selectOption({ label: 'Batch URL' });
  await page.getByRole('button', { name: 'Web Scrape' }).click();
  await page.getByRole('button', { name: 'Run Scrape' }).click();

  await expect(page.getByText('Successfully ran Web Scrape on page 1.')).toBeVisible();
  await expect(page.getByText('Alpha Item')).toBeVisible();
});
