from flask import Flask, render_template, abort, session
from flask_socketio import SocketIO, send
from flask_sqlalchemy import SQLAlchemy
import random

app = Flask(__name__)
app.config['SECRET_KEY'] = 'super_secret_key'  # Обязательно для работы сессий
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///messenger.db'
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = False

# Инициализация базы и SocketIO
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
def ensure_session_user():
    if 'user_id' not in session:
        session['user_id'] = random.randint(1000, 9999)

    if not User.query.get(session['user_id']):
        user = User(
            id=session['user_id'],
            username=f"Guest_{session['user_id']}",
            avatar_url="default.png",
            is_deleted=False,
        )
        db.session.add(user)
        db.session.commit()


@app.route('/')
def index():
    current_id = session.get('user_id')
    others = {
        user.id: user
        for user in User.query.filter(User.id != current_id).all()
    }
    return render_template('chat.html', other=others, my_id=current_id)


@app.route('/chat/<int:user_id>')
def chat(user_id):
    recipient = User.query.get(user_id)
    if not recipient:
        abort(404)

    current_id = session.get('user_id')
    others = {
        user.id: user
        for user in User.query.filter(User.id != current_id).all()
    }

    display_name = "Удалённый аккаунт" if recipient.is_deleted else recipient.username
    avatar = recipient.avatar_url or 'default.png'

    return render_template(
        'chat.html',
        other=others,
        display_name=display_name,
        avatar=avatar,
        my_id=current_id,
        recipient_id=user_id,
    )

@socketio.on('message')
def handle_message(msg):
    print(f"Получено сообщение: {msg}")
    send(msg, broadcast=True)  # Отправляем всем (включая отправителя)


if __name__ == '__main__':
    socketio.run(app, debug=True)