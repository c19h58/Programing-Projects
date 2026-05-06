from flask import Flask, render_template, abort, session, redirect, url_for
from flask_socketio import SocketIO, send
import random

app = Flask(__name__)
app.config['SECRET_KEY'] = 'super_secret_key' # Обязательно для работы сессий
socketio = SocketIO(app)

# Наш временный "склад" пользователей в памяти сервера
TEMP_USERS = {
    }

@app.before_request
def assign_temporary_id():
    """Если у зашедшего нет ID, даем ему случайный"""
    if 'my_id' not in session:
        session['my_id'] = random.randint(1000, 9999)
        # Можно даже добавить его в наш список, чтобы другие его "видели"
        TEMP_USERS[session['my_id']] = {
            "username": f"Guest_{session['my_id']}",
            "avatar_url": "default.png",
            "is_deleted": False
        }

@app.route('/')
def index():
    # Показываем список доступных чатов (всех, кроме себя)
    others = {uid: info for uid, info in TEMP_USERS.items() if uid != session.get('my_id')}
    return render_template('chat.html', other=others, my_id=session.get('my_id'))

@app.route('/chat/<int:user_id>')
def chat(user_id):
    # 1. Находим того, кому пишем
    recipient = TEMP_USERS.get(user_id)
    if not recipient:
        abort(404)

    # 2. СНОВА подготавливаем список всех остальных для боковой панели
    # (иначе цикл в HTML выдаст ошибку UndefinedError)
    others = {uid: info for uid, info in TEMP_USERS.items() if uid != session.get('user_id')}
    
    # 3. Определяем данные для плашки (как в ТГ)
    display_name = "Удалённый аккаунт" if recipient.get('is_deleted') else recipient['username']
    avatar = recipient.get('avatar_url', 'default.png')

    # ВАЖНО: передаем 'other' здесь тоже!
    return render_template(
        'chat.html', 
        other=others,           # <--- Ошибка была тут: переменная отсутствовала
        display_name=display_name, 
        avatar=avatar,
        my_id=session.get('user_id'),
        recipient_id=user_id
    )

@socketio.on('message')
def handle_message(msg):
    print(f"Получено сообщение: {msg}")
    send(msg, broadcast=True)  # Отправляем всем (включая отправителя)


if __name__ == '__main__':
    app.run(debug=True)