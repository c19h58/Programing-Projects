# database/models.py
import dbm
from sqlite3 import dbapi2


class User(dbm.Model):  # Проверь, что имя совпадает один в один
    id = dbm.Column(dbm.Integer, primary_key=True)
    # ... остальной код