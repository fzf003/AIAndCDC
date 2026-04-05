from http.server import HTTPServer, BaseHTTPRequestHandler
import json
from datetime import datetime

class Handler(BaseHTTPRequestHandler):
    def log_message(self, format, *args):
        pass
    
    def do_POST(self):
        content_length = int(self.headers.get('Content-Length', 0))
        body = self.rfile.read(content_length)
        timestamp = datetime.now().strftime('%H:%M:%S')
        print(f"[{timestamp}] CDC: {content_length} bytes")
        try:
            data = json.loads(body)
            op = data.get('payload', {}).get('op', '?')
            table = data.get('payload', {}).get('source', {}).get('table', '?')
            print(f"  -> {table} [{op}]")
        except:
            print(f"  -> {body.decode()[:100]}")
        self.send_response(200)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(b'{"status":"ok"}')
    
    def do_GET(self):
        self.send_response(200)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(b'{"status":"ready"}')

if __name__ == '__main__':
    server = HTTPServer(('0.0.0.0', 8888), Handler)
    print("CDC server listening on :8888")
    server.serve_forever()
