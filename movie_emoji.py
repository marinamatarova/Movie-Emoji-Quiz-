# movie_emoji.py
#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import sys
import os
import random
import json
import time
import threading
from pathlib import Path

# ANSI-цвета
COLORS = {
    'reset': '\033[0m',
    'green': '\033[92m',
    'red': '\033[91m',
    'yellow': '\033[93m',
    'blue': '\033[94m',
    'cyan': '\033[96m',
    'bold': '\033[1m'
}

def colorize(text, color):
    return f"{COLORS.get(color, '')}{text}{COLORS['reset']}"

# Словарь фильмов: (эмодзи, название, подсказка)
MOVIES = {
    'easy': [
        ('🦁👑', 'Король Лев', 'Мультфильм Диснея'),
        ('🚀🌌', 'Звёздные войны', 'Космическая сага'),
        ('🧙‍♂️💍', 'Властелин колец', 'Фэнтези-трилогия'),
        ('🦇🃏', 'Тёмный рыцарь', 'Фильм о Бэтмене'),
        ('🤖🔫', 'Терминатор', 'Культовый фантастический боевик'),
        ('🦈🌊', 'Челюсти', 'Фильм ужасов о большой белой акуле'),
        ('👽👾', 'Инопланетянин', 'Стивен Спилберг'),
        ('🚢💖', 'Титаник', 'Романтическая драма'),
        ('🐉⚔️', 'Гарри Поттер', 'Фильм о волшебнике'),
        ('🧟‍♂️🧠', 'Зомбиленд', 'Кинокомедия про зомби'),
    ],
    'medium': [
        ('🔫🐟', 'Крепкий орешек', 'Боевик с Брюсом Уиллисом'),
        ('👻🏠', 'Оно', 'Фильм ужасов о клоуне'),
        ('🧜‍♀️🧽', 'Губка Боб', 'Мультфильм о морских обитателях'),
        ('🐺📈', 'Волк с Уолл-стрит', 'Драма о финансисте'),
        ('🌪️🏠', 'Унесённые ветром', 'Классическая мелодрама'),
        ('🐧🎬', 'Мадагаскар', 'Мультфильм о животных'),
        ('🤡🎈', 'Оно', 'Фильм ужасов'),
        ('🧛‍♂️🌙', 'Дракула', 'Готический фильм ужасов'),
        ('🚗💨', 'Форсаж', 'Скоростной боевик'),
        ('🔪😱', 'Пила', 'Фильм ужасов'),
    ],
    'hard': [
        ('🦑🐟', 'В поисках Немо', 'Мультфильм о рыбках'),
        ('🐭🍝', 'Рататуй', 'Мультфильм о крысе-поваре'),
        ('🎪🎭', 'Безумный Макс', 'Постапокалиптический боевик'),
        ('🚲🎬', 'Велосипедист', 'Драма о гонщике'),
        ('🐺📖', 'Американский психопат', 'Психологический триллер'),
        ('👨‍🦳🧠', 'Игры разума', 'Драма о гениальном математике'),
        ('🕵️‍♂️🔍', 'Шерлок Холмс', 'Детективный фильм'),
        ('🧛‍♂️❤️', 'Сумерки', 'Фильм о вампирах'),
        ('🦍🏢', 'Кинг-Конг', 'Приключенческий фильм'),
        ('👽🌍', 'День независимости', 'Фантастический фильм'),
    ]
}

class MovieEmojiGame:
    def __init__(self, level='easy'):
        self.level = level
        self.movies = MOVIES[level]
        self.max_movies = 10
        self.score = 0
        self.correct = 0
        self.skipped = 0
        self.hint_used = 0
        self.stats_file = Path.home() / '.movie_emoji_stats.json'
        self.load_stats()
        self.time_limit = {'easy': 60, 'medium': 45, 'hard': 30}[level]

    def load_stats(self):
        if self.stats_file.exists():
            with open(self.stats_file, 'r') as f:
                self.stats = json.load(f)
        else:
            self.stats = {'games': 0, 'best_score': 0, 'total_movies': 0}

    def save_stats(self):
        with open(self.stats_file, 'w') as f:
            json.dump(self.stats, f, indent=2)

    def timer_input(self, prompt, timeout):
        print(colorize(prompt, 'bold'), end='', flush=True)
        user_input = ['']
        def get_input():
            try:
                user_input[0] = sys.stdin.readline().strip()
            except:
                pass
        thread = threading.Thread(target=get_input)
        thread.daemon = True
        thread.start()
        start = time.time()
        while time.time() - start < timeout:
            if user_input[0] != '':
                return user_input[0]
            remaining = int(timeout - (time.time() - start))
            sys.stdout.write(f"\r{colorize(f'Осталось времени: {remaining} сек', 'yellow')}")
            sys.stdout.flush()
            time.sleep(1)
        return None

    def show_hint(self, movie):
        # Показываем первую букву или количество букв
        title = movie[1]
        print(colorize(f"Подсказка: первая буква '{title[0]}', всего {len(title)} букв", 'green'))

    def play_round(self, movie):
        emoji, title, hint = movie
        print(colorize(f"\n🎬 Эмодзи: {emoji}", 'cyan'))
        print(colorize(f"Подсказка: {hint}", 'blue'))
        print("Введите название фильма, ? для подсказки, pass для пропуска, quit для выхода.")

        start_time = time.time()
        answer = self.timer_input("Ваш ответ: ", self.time_limit)
        elapsed = time.time() - start_time

        if answer is None:
            print(colorize("⏰ Время вышло!", 'red'))
            self.skipped += 1
            print(colorize(f"Загаданный фильм: {title}", 'yellow'))
            return

        if answer == 'quit':
            print("Выход.")
            self.save_stats()
            sys.exit(0)
        if answer == '?':
            if self.hint_used < 1:
                self.show_hint(movie)
                self.hint_used += 1
                self.score = max(0, self.score - 5)
                self.play_round(movie)
                return
            else:
                print(colorize("Подсказка уже использована.", 'yellow'))
                self.play_round(movie)
                return
        if answer == 'pass':
            self.skipped += 1
            print(colorize(f"Загаданный фильм: {title}", 'yellow'))
            return

        if answer.lower() == title.lower():
            self.correct += 1
            points = max(1, int(10 - elapsed / 10))
            self.score += points
            print(colorize(f"✅ Верно! +{points} очков. Время: {elapsed:.1f} сек", 'green'))
        else:
            self.skipped += 1
            print(colorize(f"❌ Неверно. Загаданный фильм: {title}", 'red'))

    def play(self):
        print(colorize("🎬 Добро пожаловать в игру 'Угадай фильм по эмодзи'!", 'bold'))
        print(f"Уровень: {self.level}, лимит времени: {self.time_limit} сек.")
        print("Цель: угадать фильм по набору эмодзи.")
        print("Вводите название, используйте ? для подсказки, pass для пропуска, quit для выхода.\n")

        random.shuffle(self.movies)
        total = min(self.max_movies, len(self.movies))
        for i in range(total):
            print(colorize(f"\nФильм {i+1}/{total}", 'blue'))
            self.play_round(self.movies[i])

        print(colorize("\n🏁 Игра завершена!", 'bold'))
        print(f"  Угадано фильмов: {self.correct}")
        print(f"  Пропущено: {self.skipped}")
        print(f"  Использовано подсказок: {self.hint_used}")
        print(f"  Счёт: {self.score}")
        self.stats['games'] += 1
        if self.score > self.stats['best_score']:
            self.stats['best_score'] = self.score
            print(colorize("🏆 Новый рекорд!", 'green'))
        self.stats['total_movies'] += self.correct
        self.save_stats()
        print(colorize(f"Лучший результат: {self.stats['best_score']}", 'yellow'))

def main():
    level = 'easy'
    show_stats = False
    reset_stats = False
    args = sys.argv[1:]
    for arg in args:
        if arg in ['easy', 'medium', 'hard']:
            level = arg
        elif arg == '-s' or arg == '--stats':
            show_stats = True
        elif arg == '-r' or arg == '--reset':
            reset_stats = True
        elif arg == '-h' or arg == '--help':
            print("Usage: movie_emoji.py [easy|medium|hard] [-s] [-r]")
            return
    if reset_stats:
        stats_file = Path.home() / '.movie_emoji_stats.json'
        if stats_file.exists():
            stats_file.unlink()
        print("Статистика сброшена.")
        return
    if show_stats:
        stats_file = Path.home() / '.movie_emoji_stats.json'
        if stats_file.exists():
            with open(stats_file, 'r') as f:
                stats = json.load(f)
                print(colorize("📊 Статистика:", 'bold'))
                print(f"  Сыграно игр: {stats['games']}")
                print(f"  Лучший счёт: {stats['best_score']}")
                print(f"  Всего угадано фильмов: {stats['total_movies']}")
        else:
            print("Статистика пуста.")
        return
    game = MovieEmojiGame(level)
    game.play()

if __name__ == '__main__':
    try:
        main()
    except KeyboardInterrupt:
        print(colorize("\nИгра прервана.", 'yellow'))
        sys.exit(0)
