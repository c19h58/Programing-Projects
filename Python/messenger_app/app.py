from flask import Flask, render_template
from flask_socketio import SocketIO, send

app = Flask(__name__)
app.config['SECRET_KEY'] = 'your_secret_key'  # Замени на свой секретный ключ
socketio = SocketIO(app)

@app.route('/')
def index():
    return render_template('chat.html')

# Обработка входящих сообщений
@socketio.on('message')
def handle_message(msg):
    print(f"Сообщение: {msg}")
    # Рассылаем сообщение всем подключенным клиентам
    send(msg, broadcast=True)

if __name__ == '__main__':
    socketio.run(app, debug=True)
    