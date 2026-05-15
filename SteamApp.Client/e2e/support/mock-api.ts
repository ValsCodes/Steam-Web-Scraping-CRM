import { expect, Page, Route } from '@playwright/test';

type MockApiState = {
  authHeaders: string[];
  games: Array<{
    id: number;
    name: string;
    baseUrl: string;
    pageUrl: string | null;
    internalId: number | null;
    isActive: boolean;
  }>;
};

const corsHeaders = {
  'access-control-allow-origin': '*',
  'access-control-allow-headers': 'authorization, content-type',
  'access-control-allow-methods': 'GET, POST, PUT, PATCH, DELETE, OPTIONS',
};

export async function mockSteamApi(page: Page): Promise<MockApiState> {
  const state: MockApiState = {
    authHeaders: [],
    games: [
      {
        id: 1,
        name: 'Alpha Game',
        baseUrl: 'https://steamcommunity.com',
        pageUrl: 'https://steamcommunity.com/app/440',
        internalId: 440,
        isActive: true,
      },
      {
        id: 2,
        name: 'Beta Game',
        baseUrl: 'https://steamcommunity.com',
        pageUrl: 'https://steamcommunity.com/app/570',
        internalId: 570,
        isActive: false,
      },
    ],
  };

  await page.route('https://localhost:7273/**', async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    const method = request.method();
    const pathname = url.pathname.toLowerCase();

    if (method === 'OPTIONS') {
      await route.fulfill({ status: 204, headers: corsHeaders });
      return;
    }

    const authorization = request.headers()['authorization'];
    if (authorization) {
      state.authHeaders.push(authorization);
    }

    if (pathname === '/api/auth/login' && method === 'POST') {
      await fulfillJson(route, {
        token: createJwt({
          sub: 'e2e-user',
          name: 'E2E User',
          email: 'e2e@example.test',
          exp: Math.floor(Date.now() / 1000) + 3600,
        }),
      });
      return;
    }

    if (pathname === '/api/games' && method === 'GET') {
      await fulfillJson(route, state.games);
      return;
    }

    if (pathname === '/api/games' && method === 'POST') {
      const body = request.postDataJSON() as {
        name: string;
        pageUrl: string | null;
        internalId: number | null;
        isActive: boolean;
      };
      const created = {
        id: Math.max(...state.games.map((game) => game.id)) + 1,
        name: body.name,
        baseUrl: 'https://steamcommunity.com',
        pageUrl: body.pageUrl,
        internalId: Number(body.internalId),
        isActive: body.isActive,
      };
      state.games.push(created);
      await fulfillJson(route, created, 201);
      return;
    }

    if (pathname === '/api/game-urls' && method === 'GET') {
      await fulfillJson(route, [
        {
          id: 10,
          name: 'Batch URL',
          gameId: 1,
          gameName: 'Alpha Game',
          scrapingModeId: 2,
          scrapingModeName: 'Batch',
          partialUrl: 'https://steamcommunity.com/market/search?page={0}',
          isActive: true,
        },
      ]);
      return;
    }

    if (pathname === '/api/scraping-modes' && method === 'GET') {
      await fulfillJson(route, [
        { id: 1, name: 'Manual Batch' },
        { id: 2, name: 'Batch' },
        { id: 3, name: 'Pixel Batch' },
        { id: 4, name: 'Public API' },
      ]);
      return;
    }

    if (pathname === '/steam/scrape-page/gameurl/10/page/1' && method === 'GET') {
      await fulfillJson(route, [
        {
          name: 'Alpha Item',
          price: 1.23,
          imageUrl: 'https://steamcommunity.com/image.png',
          quantity: 3,
          pixelName: '',
          linkUrl: 'https://steamcommunity.com/market/listings/440/Alpha%20Item',
          pageUrl: 'https://steamcommunity.com/market/search?page=1',
          redValue: null,
          greenValue: null,
          blueValue: null,
          isPainted: false,
        },
      ]);
      return;
    }

    await fulfillJson(route, []);
  });

  return state;
}

export async function signIn(page: Page): Promise<void> {
  await page.goto('/login');
  await page.getByLabel('Email or username').fill('e2e@example.test');
  await page.getByLabel('Password').fill('Password1');
  await page.getByRole('button', { name: 'Log in' }).click();
  await expect(page.getByRole('heading', { name: 'Games' })).toBeVisible();
}

async function fulfillJson(route: Route, json: unknown, status = 200): Promise<void> {
  await route.fulfill({
    status,
    headers: corsHeaders,
    json,
  });
}

function createJwt(payload: Record<string, unknown>): string {
  const encode = (value: Record<string, unknown>) =>
    Buffer.from(JSON.stringify(value)).toString('base64url');

  return `${encode({ alg: 'none', typ: 'JWT' })}.${encode(payload)}.signature`;
}
