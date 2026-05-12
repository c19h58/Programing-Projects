from flask import Flask, render_template, abort, session, redirect, url_for
from flask_socketio import SocketIO, send
from flask_sqlalchemy import SQLAlchemy
import random

app = Flask(__name__)
app.config['SECRET_KEY'] = 'super_secret_key' # Обязательно для работы сессий
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///messenger.db'


db = SQLAlchemy(app)
socketio = SocketIO(app)

class User(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    username = db.Column(db.String(80), unique=True, nullable=False)
    avatar_url = db.Column(db.String(200), nullable=True)
    is_deleted = db.Column(db.Boolean, default=False)



with app.app_context():
    db.create_all()  # Создаем таблицы, если их нет
    if not User.query.get(1):
        deleted_bot = User(id=1, username="Удалённый аккаунт", is_deleted=True, avatar_url="ghost.png")
        db.session.add(deleted_bot)
        db.session.commit()

@app.before_request
def manage_temp_user():
    """Если у зашедшего нет ID, даем ему случайный"""
    if 'my_id' not in session:
        session['my_id'] = random.randint(1000, 9999)
        # Можно даже добавить его в наш список, чтобы другие его "видели"
        user = User(
            username=f"Guest_{session['my_id']}",
            avatar_url="default.png",
            is_deleted=False
        )
        db.session.add(user)
        db.session.commit()
@app.route('/')
def index():
    # Показываем список доступных чатов (всех, кроме себя)
    others = {uid: info for uid, info in User.query.all() if uid != session.get('my_id')}
    return render_template('chat.html', other=others, my_id=session.get('my_id'))

@app.route('/chat/<int:user_id>')
def chat(user_id):
    # 1. Находим того, кому пишем
    recipient = User.query.get(user_id)
    if not recipient:
        abort(404)

    # 2. СНОВА подготавливаем список всех остальных для боковой панели
    # (иначе цикл в HTML выдаст ошибку UndefinedError)
    others = {uid: info for uid, info in User.query.all() if uid != session.get('my_id')}
    
    # 3. Определяем данные для плашки (как в ТГ)
    display_name = "Удалённый аккаунт" if recipient.is_deleted else recipient.username
    avatar = recipient.avatar_url if recipient.avatar_url else 'default.png'

    # ВАЖНО: передаем 'other' здесь тоже!
    return render_template(
        'chat.html', 
        other=others,           # <--- Ошибка была тут: переменная отсутствовала
        display_name=display_name, 
        avatar=avatar,
        my_id=session.get('my_id'),
        recipient_id=user_id
    )

@socketio.on('message')
def handle_message(msg):
    print(f"Получено сообщение: {msg}")
    send(msg, broadcast=True)  # Отправляем всем (включая отправителя)


if __name__ == '__main__':
    app.run(debug=True)