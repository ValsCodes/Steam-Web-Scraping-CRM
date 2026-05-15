import { expect, test } from '@playwright/test';

import { mockSteamApi } from './support/mock-api';

test('redirects protected routes to login and sends authenticated catalog requests after sign in', async ({ page }) => {
  const api = await mockSteamApi(page);

  await page.goto('/games');
  await expect(page).toHaveURL(/\/login$/);

  await page.getByLabel('Email or username').fill('e2e@example.test');
  await page.getByLabel('Password').fill('Password1');
  await page.getByRole('button', { name: 'Log in' }).click();

  await expect(page.getByRole('heading', { name: 'Games' })).toBeVisible();
  await expect(page.getByText('Alpha Game')).toBeVisible();
  await expect.poll(() => api.authHeaders.some((header) => header.startsWith('Bearer '))).toBe(true);
});
