import { expect, test } from '@playwright/test';

import { mockSteamApi, signIn } from './support/mock-api';

test('creates a game through the catalog form and returns to the games grid', async ({ page }) => {
  await mockSteamApi(page);
  await signIn(page);

  await page.getByRole('button', { name: 'Create' }).click();
  await expect(page.getByRole('heading', { name: 'Create Game' })).toBeVisible();

  await page.locator('#game-name').fill('Created E2E Game');
  await page.locator('#game-internal-id').fill('999');
  await page.locator('#game-page-url').fill('https://steamcommunity.com/app/999');
  await page.getByRole('button', { name: 'Create game' }).click();

  await expect(page.getByRole('heading', { name: 'Games' })).toBeVisible();
  await expect(page.getByText('Created E2E Game')).toBeVisible();
});
