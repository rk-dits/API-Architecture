import http from "k6/http";
import { check, sleep } from "k6";

export let options = {
  vus: 50,
  duration: "1m",
  thresholds: {
    http_req_duration: ["p(95)<500"],
    http_req_failed: ["rate<0.01"],
  },
};

export default function () {
  const res = http.get("http://localhost:5000/health");
  check(res, { "status is 200": (r) => r.status === 200 });
  sleep(1);
}
