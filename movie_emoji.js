// movie_emoji.js
#!/usr/bin/env node
'use strict';

const fs = require('fs');
const path = require('path');
const os = require('os');
const readline = require('readline');

const COLORS = {
    reset: '\x1b[0m',
    green: '\x1b[92m',
    red: '\x1b[91m',
    yellow: '\x1b[93m',
    blue: '\x1b[94m',
    cyan: '\x1b[96m',
    bold: '\x1b[1m'
};

function colorize(text, color) {
    return COLORS[color] + text + COLORS.reset;
}

const MOVIES = {
    easy: [
        { emoji: '🦁👑', title: 'Король Лев', hint: 'Мультфильм Диснея' },
        { emoji: '🚀🌌', title: 'Звёздные войны', hint: 'Космическая сага' },
        { emoji: '🧙‍♂️💍', title: 'Властелин колец', hint: 'Фэнтези-трилогия' },
        { emoji: '🦇🃏', title: 'Тёмный рыцарь', hint: 'Фильм о Бэтмене' },
        { emoji: '🤖🔫', title: 'Терминатор', hint: 'Культовый фантастический боевик' },
        { emoji: '🦈🌊', title: 'Челюсти', hint: 'Фильм ужасов о большой белой акуле' },
        { emoji: '👽👾', title: 'Инопланетянин', hint: 'Стивен Спилберг' },
        { emoji: '🚢💖', title: 'Титаник', hint: 'Романтическая драма' },
        { emoji: '🐉⚔️', title: 'Гарри Поттер', hint: 'Фильм о волшебнике' },
        { emoji: '🧟‍♂️🧠', title: 'Зомбиленд', hint: 'Кинокомедия про зомби' },
    ],
    medium: [
        { emoji: '🔫🐟', title: 'Крепкий орешек', hint: 'Боевик с Брюсом Уиллисом' },
        { emoji: '👻🏠', title: 'Оно', hint: 'Фильм ужасов о клоуне' },
        { emoji: '🧜‍♀️🧽', title: 'Губка Боб', hint: 'Мультфильм о морских обитателях' },
        { emoji: '🐺📈', title: 'Волк с Уолл-стрит', hint: 'Драма о финансисте' },
        { emoji: '🌪️🏠', title: 'Унесённые ветром', hint: 'Классическая мелодрама' },
        { emoji: '🐧🎬', title: 'Мадагаскар', hint: 'Мультфильм о животных' },
        { emoji: '🤡🎈', title: 'Оно', hint: 'Фильм ужасов' },
        { emoji: '🧛‍♂️🌙', title: 'Дракула', hint: 'Готический фильм ужасов' },
        { emoji: '🚗💨', title: 'Форсаж', hint: 'Скоростной боевик' },
        { emoji: '🔪😱', title: 'Пила', hint: 'Фильм ужасов' },
    ],
    hard: [
        { emoji: '🦑🐟', title: 'В поисках Немо', hint: 'Мультфильм о рыбках' },
        { emoji: '🐭🍝', title: 'Рататуй', hint: 'Мультфильм о крысе-поваре' },
        { emoji: '🎪🎭', title: 'Безумный Макс', hint: 'Постапокалиптический боевик' },
        { emoji: '🚲🎬', title: 'Велосипедист', hint: 'Драма о гонщике' },
        { emoji: '🐺📖', title: 'Американский психопат', hint: 'Психологический триллер' },
        { emoji: '👨‍🦳🧠', title: 'Игры разума', hint: 'Драма о гениальном математике' },
        { emoji: '🕵️‍♂️🔍', title: 'Шерлок Холмс', hint: 'Детективный фильм' },
        { emoji: '🧛‍♂️❤️', title: 'Сумерки', hint: 'Фильм о вампирах' },
        { emoji: '🦍🏢', title: 'Кинг-Конг', hint: 'Приключенческий фильм' },
        { emoji: '👽🌍', title: 'День независимости', hint: 'Фантастический фильм' },
    ]
};

class MovieEmojiGame {
    constructor(level = 'easy') {
        this.level = level;
        this.movies = MOVIES[level];
        this.maxMovies = 10;
        this.score = 0;
        this.correct = 0;
        this.skipped = 0;
        this.hintUsed = 0;
        this.statsFile = path.join(os.homedir(), '.movie_emoji_stats.json');
        this.loadStats();
        this.timeLimit = { easy: 60, medium: 45, hard: 30 }[level];
    }

    loadStats() {
        try {
            this.stats = JSON.parse(fs.readFileSync(this.statsFile, 'utf8'));
        } catch {
            this.stats = { games: 0, best_score: 0, total_movies: 0 };
        }
    }

    saveStats() {
        fs.writeFileSync(this.statsFile, JSON.stringify(this.stats, null, 2));
    }

    timerInput(prompt, timeout) {
        return new Promise((resolve) => {
            const rl = readline.createInterface({
                input: process.stdin,
                output: process.stdout
            });
            let answered = false;
            const timer = setTimeout(() => {
                if (!answered) {
                    rl.close();
                    resolve('');
                }
            }, timeout * 1000);
            rl.question(colorize(prompt, 'bold'), (answer) => {
                answered = true;
                clearTimeout(timer);
                rl.close();
                resolve(answer.trim());
            });
        });
    }

    showHint(movie) {
        console.log(colorize(`Подсказка: первая буква '${movie.title[0]}', всего ${movie.title.length} букв`, 'green'));
    }

    async playRound(movie) {
        console.log(colorize(`\n🎬 Эмодзи: ${movie.emoji}`, 'cyan'));
        console.log(colorize(`Подсказка: ${movie.hint}`, 'blue'));
        console.log('Введите название фильма, ? для подсказки, pass для пропуска, quit для выхода.');

        const start = Date.now();
        const answer = await this.timerInput('Ваш ответ: ', this.timeLimit);
        const elapsed = (Date.now() - start) / 1000;

        if (answer === '') {
            console.log(colorize('⏰ Время вышло!', 'red'));
            this.skipped++;
            console.log(colorize(`Загаданный фильм: ${movie.title}`, 'yellow'));
            return;
        }
        if (answer === 'quit') {
            console.log('Выход.');
            this.saveStats();
            process.exit(0);
        }
        if (answer === '?') {
            if (this.hintUsed < 1) {
                this.showHint(movie);
                this.hintUsed++;
                this.score = Math.max(0, this.score - 5);
                await this.playRound(movie);
                return;
            } else {
                console.log(colorize('Подсказка уже использована.', 'yellow'));
                await this.playRound(movie);
                return;
            }
        }
        if (answer === 'pass') {
            this.skipped++;
            console.log(colorize(`Загаданный фильм: ${movie.title}`, 'yellow'));
            return;
        }
        if (answer.toLowerCase() === movie.title.toLowerCase()) {
            this.correct++;
            const points = Math.max(1, Math.floor(10 - elapsed / 10));
            this.score += points;
            console.log(colorize(`✅ Верно! +${points} очков. Время: ${elapsed.toFixed(1)} сек`, 'green'));
        } else {
            this.skipped++;
            console.log(colorize(`❌ Неверно. Загаданный фильм: ${movie.title}`, 'red'));
        }
    }

    async play() {
        console.log(colorize('🎬 Добро пожаловать в игру "Угадай фильм по эмодзи"!', 'bold'));
        console.log(`Уровень: ${this.level}, лимит времени: ${this.timeLimit} сек.`);
        console.log('Цель: угадать фильм по набору эмодзи.');
        console.log('Вводите название, используйте ? для подсказки, pass для пропуска, quit для выхода.\n');

        // Перемешиваем
        for (let i = this.movies.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [this.movies[i], this.movies[j]] = [this.movies[j], this.movies[i]];
        }
        const total = Math.min(this.maxMovies, this.movies.length);
        for (let i = 0; i < total; i++) {
            console.log(colorize(`\nФильм ${i+1}/${total}`, 'blue'));
            await this.playRound(this.movies[i]);
        }
        console.log(colorize('\n🏁 Игра завершена!', 'bold'));
        console.log(`  Угадано фильмов: ${this.correct}`);
        console.log(`  Пропущено: ${this.skipped}`);
        console.log(`  Использовано подсказок: ${this.hintUsed}`);
        console.log(`  Счёт: ${this.score}`);
        this.stats.games++;
        if (this.score > this.stats.best_score) {
            this.stats.best_score = this.score;
            console.log(colorize('🏆 Новый рекорд!', 'green'));
        }
        this.stats.total_movies += this.correct;
        this.saveStats();
        console.log(colorize(`Лучший результат: ${this.stats.best_score}`, 'yellow'));
    }
}

async function main() {
    let level = 'easy';
    let showStats = false;
    let resetStats = false;
    const args = process.argv.slice(2);
    for (const arg of args) {
        if (arg === 'easy' || arg === 'medium' || arg === 'hard') level = arg;
        else if (arg === '-s' || arg === '--stats') showStats = true;
        else if (arg === '-r' || arg === '--reset') resetStats = true;
        else if (arg === '-h' || arg === '--help') {
            console.log('Usage: node movie_emoji.js [easy|medium|hard] [-s] [-r]');
            process.exit(0);
        }
    }
    if (resetStats) {
        const f = path.join(os.homedir(), '.movie_emoji_stats.json');
        if (fs.existsSync(f)) fs.unlinkSync(f);
        console.log('Статистика сброшена.');
        return;
    }
    if (showStats) {
        const f = path.join(os.homedir(), '.movie_emoji_stats.json');
        try {
            const stats = JSON.parse(fs.readFileSync(f, 'utf8'));
            console.log(colorize('📊 Статистика:', 'bold'));
            console.log(`  Сыграно игр: ${stats.games}`);
            console.log(`  Лучший счёт: ${stats.best_score}`);
            console.log(`  Всего угадано фильмов: ${stats.total_movies}`);
        } catch {
            console.log('Статистика пуста.');
        }
        return;
    }
    const game = new MovieEmojiGame(level);
    await game.play();
}

main().catch(console.error);
