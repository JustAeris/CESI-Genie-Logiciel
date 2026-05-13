import http.server
import json
import os
import datetime

LOG_DIR = os.environ.get("LOG_DIR", "./logs")
PORT = 5000

os.makedirs(LOG_DIR, exist_ok=True)


class LogHandler(http.server.BaseHTTPRequestHandler):
    def do_POST(self):
        if self.path != "/logs":
            self.send_response(404)
            self.end_headers()
            return

        length = int(self.headers.get("Content-Length", 0))
        body = self.rfile.read(length)

        try:
            entry = json.loads(body)
            date_str = datetime.date.today().isoformat()
            log_file = os.path.join(LOG_DIR, f"{date_str}.json")

            entries = []
            if os.path.exists(log_file):
                with open(log_file, "r", encoding="utf-8") as f:
                    entries = json.load(f)

            entries.append(entry)

            with open(log_file, "w", encoding="utf-8") as f:
                json.dump(entries, f, indent=2, ensure_ascii=False)

            self.send_response(200)
            self.end_headers()
        except Exception:
            self.send_response(500)
            self.end_headers()

    def log_message(self, format, *args):
        pass  # Suppress default per-request console output


if __name__ == "__main__":
    with http.server.HTTPServer(("", PORT), LogHandler) as server:
        print(f"Log server listening on port {PORT} — writing to {LOG_DIR}", flush=True)
        server.serve_forever()
