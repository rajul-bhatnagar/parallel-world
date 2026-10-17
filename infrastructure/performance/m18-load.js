import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
  scenarios: {
    representative: {
      executor: 'constant-vus',
      vus: 25,
      duration: '2m',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.01'],
    'http_req_duration{kind:crud}': ['p(95)<500'],
    'http_req_duration{kind:catchup}': ['p(95)<5000'],
  },
};

const baseUrl = __ENV.BASE_URL;
const accessToken = __ENV.ACCESS_TOKEN;
const worldId = __ENV.WORLD_ID;

export function setup() {
  if (!baseUrl || !accessToken || !worldId) {
    throw new Error('BASE_URL, ACCESS_TOKEN, and WORLD_ID are required.');
  }
}

export default function () {
  const headers = { Authorization: `Bearer ${accessToken}` };
  const feed = http.get(`${baseUrl}/api/v1/worlds/${worldId}/feed`, {
    headers,
    tags: { kind: 'crud' },
  });
  check(feed, { 'feed is successful': (response) => response.status === 200 });

  if (__ITER % 10 === 0) {
    const catchUp = http.post(
      `${baseUrl}/api/v1/worlds/${worldId}/catch-up`,
      null,
      { headers, tags: { kind: 'catchup' } },
    );
    check(catchUp, {
      'catch-up is accepted': (response) =>
        response.status === 200 || response.status === 202 || response.status === 409,
    });
  }

  sleep(1);
}
