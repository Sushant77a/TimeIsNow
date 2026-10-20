import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  retries: 0,
  workers: 3,
  reporter: [['list']],
  timeout: 30_000,
  expect: { timeout: 8_000 },
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  webServer: [
    {
      command:
        'dotnet TimeIsNow.API.dll --urls http://localhost:5099',
      cwd: '../src/TimeIsNow.API/bin/Debug/net10.0',
      env: { ASPNETCORE_ENVIRONMENT: 'Development' },
      url: 'http://localhost:5099/api/health',
      reuseExistingServer: true,
      timeout: 120_000,
    },
    {
      command: 'npm run dev',
      url: 'http://localhost:5173',
      reuseExistingServer: true,
      timeout: 120_000,
    },
  ],
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
})
