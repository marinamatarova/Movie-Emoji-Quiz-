// movie_emoji.cpp
#include <iostream>
#include <vector>
#include <string>
#include <map>
#include <random>
#include <algorithm>
#include <chrono>
#include <thread>
#include <fstream>
#include <cctype>
#include <filesystem>

using namespace std;
namespace fs = std::filesystem;

const string RESET = "\033[0m";
const string GREEN = "\033[92m";
const string RED = "\033[91m";
const string YELLOW = "\033[93m";
const string BLUE = "\033[94m";
const string CYAN = "\033[96m";
const string BOLD = "\033[1m";

string colorize(const string& text, const string& color) {
    return color + text + RESET;
}

string getHomeDir() {
    const char* home = getenv("HOME");
    if (!home) home = getenv("USERPROFILE");
    return string(home);
}

struct Movie {
    string emoji;
    string title;
    string hint;
};

map<string, vector<Movie>> MOVIES = {
    {"easy", {
        {"🦁👑", "Король Лев", "Мультфильм Диснея"},
        {"🚀🌌", "Звёздные войны", "Космическая сага"},
        {"🧙‍♂️💍", "Властелин колец", "Фэнтези-трилогия"},
        {"🦇🃏", "Тёмный рыцарь", "Фильм о Бэтмене"},
        {"🤖🔫", "Терминатор", "Культовый фантастический боевик"},
        {"🦈🌊", "Челюсти", "Фильм ужасов о большой белой акуле"},
        {"👽👾", "Инопланетянин", "Стивен Спилберг"},
        {"🚢💖", "Титаник", "Романтическая драма"},
        {"🐉⚔️", "Гарри Поттер", "Фильм о волшебнике"},
        {"🧟‍♂️🧠", "Зомбиленд", "Кинокомедия про зомби"},
    }},
    {"medium", {
        {"🔫🐟", "Крепкий орешек", "Боевик с Брюсом Уиллисом"},
        {"👻🏠", "Оно", "Фильм ужасов о клоуне"},
        {"🧜‍♀️🧽", "Губка Боб", "Мультфильм о морских обитателях"},
        {"🐺📈", "Волк с Уолл-стрит", "Драма о финансисте"},
        {"🌪️🏠", "Унесённые ветром", "Классическая мелодрама"},
        {"🐧🎬", "Мадагаскар", "Мультфильм о животных"},
        {"🤡🎈", "Оно", "Фильм ужасов"},
        {"🧛‍♂️🌙", "Дракула", "Готический фильм ужасов"},
        {"🚗💨", "Форсаж", "Скоростной боевик"},
        {"🔪😱", "Пила", "Фильм ужасов"},
    }},
    {"hard", {
        {"🦑🐟", "В поисках Немо", "Мультфильм о рыбках"},
        {"🐭🍝", "Рататуй", "Мультфильм о крысе-поваре"},
        {"🎪🎭", "Безумный Макс", "Постапокалиптический боевик"},
        {"🚲🎬", "Велосипедист", "Драма о гонщике"},
        {"🐺📖", "Американский психопат", "Психологический триллер"},
        {"👨‍🦳🧠", "Игры разума", "Драма о гениальном математике"},
        {"🕵️‍♂️🔍", "Шерлок Холмс", "Детективный фильм"},
        {"🧛‍♂️❤️", "Сумерки", "Фильм о вампирах"},
        {"🦍🏢", "Кинг-Конг", "Приключенческий фильм"},
        {"👽🌍", "День независимости", "Фантастический фильм"},
    }}
};

class MovieEmojiGame {
public:
    string level;
    vector<Movie> movies;
    int maxMovies;
    int score;
    int correct;
    int skipped;
    int hintUsed;
    int timeLimit;
    string statsFile;
    map<string, int> stats;

    MovieEmojiGame(string lvl) : level(lvl), maxMovies(10), score(0), correct(0), skipped(0), hintUsed(0) {
        movies = MOVIES[level];
        statsFile = getHomeDir() + "/.movie_emoji_stats.json";
        loadStats();
        if (level == "easy") timeLimit = 60;
        else if (level == "medium") timeLimit = 45;
        else timeLimit = 30;
    }

    void loadStats() {
        ifstream f(statsFile);
        if (!f) {
            stats["games"] = 0; stats["best_score"] = 0; stats["total_movies"] = 0;
            return;
        }
        string content((istreambuf_iterator<char>(f)), istreambuf_iterator<char>());
        auto extract = [&](const string& key) -> int {
            size_t pos = content.find("\"" + key + "\"");
            if (pos == string::npos) return 0;
            pos = content.find(":", pos) + 1;
            size_t end = content.find(",", pos);
            if (end == string::npos) end = content.find("}", pos);
            try { return stoi(content.substr(pos, end-pos)); } catch (...) { return 0; }
        };
        stats["games"] = extract("games");
        stats["best_score"] = extract("best_score");
        stats["total_movies"] = extract("total_movies");
    }

    void saveStats() {
        ofstream f(statsFile);
        if (f) {
            f << "{\"games\":" << stats["games"] << ",\"best_score\":" << stats["best_score"]
              << ",\"total_movies\":" << stats["total_movies"] << "}";
        }
    }

    string timerInput(const string& prompt, int timeout) {
        cout << colorize(prompt, BOLD) << flush;
        string input;
        auto start = chrono::steady_clock::now();
        while (chrono::duration_cast<chrono::seconds>(chrono::steady_clock::now() - start).count() < timeout) {
            if (cin.rdbuf()->in_avail() > 0) {
                getline(cin, input);
                return input;
            }
            int remaining = timeout - chrono::duration_cast<chrono::seconds>(chrono::steady_clock::now() - start).count();
            cout << "\r" << colorize("Осталось времени: " + to_string(remaining) + " сек", YELLOW) << flush;
            this_thread::sleep_for(chrono::seconds(1));
        }
        return "";
    }

    void showHint(const Movie& movie) {
        cout << colorize("Подсказка: первая буква '" + string(1, movie.title[0]) + "', всего " + to_string(movie.title.size()) + " букв", GREEN) << endl;
    }

    void playRound(const Movie& movie) {
        cout << colorize("\n🎬 Эмодзи: " + movie.emoji, CYAN) << endl;
        cout << colorize("Подсказка: " + movie.hint, BLUE) << endl;
        cout << "Введите название фильма, ? для подсказки, pass для пропуска, quit для выхода." << endl;

        auto start = chrono::steady_clock::now();
        string answer = timerInput("Ваш ответ: ", timeLimit);
        auto elapsed = chrono::duration_cast<chrono::seconds>(chrono::steady_clock::now() - start).count();

        if (answer.empty()) {
            cout << colorize("⏰ Время вышло!", RED) << endl;
            skipped++;
            cout << colorize("Загаданный фильм: " + movie.title, YELLOW) << endl;
            return;
        }
        if (answer == "quit") {
            cout << "Выход." << endl;
            saveStats();
            exit(0);
        }
        if (answer == "?") {
            if (hintUsed < 1) {
                showHint(movie);
                hintUsed++;
                score = max(0, score - 5);
                playRound(movie);
                return;
            } else {
                cout << colorize("Подсказка уже использована.", YELLOW) << endl;
                playRound(movie);
                return;
            }
        }
        if (answer == "pass") {
            skipped++;
            cout << colorize("Загаданный фильм: " + movie.title, YELLOW) << endl;
            return;
        }
        // Сравниваем без учёта регистра
        string ansLower = answer;
        string titleLower = movie.title;
        transform(ansLower.begin(), ansLower.end(), ansLower.begin(), ::tolower);
        transform(titleLower.begin(), titleLower.end(), titleLower.begin(), ::tolower);
        if (ansLower == titleLower) {
            correct++;
            int points = max(1, (int)(10 - elapsed / 10));
            score += points;
            cout << colorize("✅ Верно! +" + to_string(points) + " очков. Время: " + to_string(elapsed) + " сек", GREEN) << endl;
        } else {
            skipped++;
            cout << colorize("❌ Неверно. Загаданный фильм: " + movie.title, RED) << endl;
        }
    }

    void play() {
        cout << colorize("🎬 Добро пожаловать в игру 'Угадай фильм по эмодзи'!", BOLD) << endl;
        cout << "Уровень: " << level << ", лимит времени: " << timeLimit << " сек." << endl;
        cout << "Цель: угадать фильм по набору эмодзи." << endl;
        cout << "Вводите название, используйте ? для подсказки, pass для пропуска, quit для выхода.\n" << endl;

        random_device rd;
        mt19937 g(rd());
        shuffle(movies.begin(), movies.end(), g);
        int total = min(maxMovies, (int)movies.size());
        for (int i=0; i<total; ++i) {
            cout << colorize("\nФильм " + to_string(i+1) + "/" + to_string(total), BLUE) << endl;
            playRound(movies[i]);
        }
        cout << colorize("\n🏁 Игра завершена!", BOLD) << endl;
        cout << "  Угадано фильмов: " << correct << endl;
        cout << "  Пропущено: " << skipped << endl;
        cout << "  Использовано подсказок: " << hintUsed << endl;
        cout << "  Счёт: " << score << endl;
        stats["games"]++;
        if (score > stats["best_score"]) {
            stats["best_score"] = score;
            cout << colorize("🏆 Новый рекорд!", GREEN) << endl;
        }
        stats["total_movies"] += correct;
        saveStats();
        cout << colorize("Лучший результат: " + to_string(stats["best_score"]), YELLOW) << endl;
    }
};

int main(int argc, char* argv[]) {
    string level = "easy";
    bool showStats = false, resetStats = false;
    for (int i=1; i<argc; ++i) {
        string arg = argv[i];
        if (arg == "easy" || arg == "medium" || arg == "hard") level = arg;
        else if (arg == "-s" || arg == "--stats") showStats = true;
        else if (arg == "-r" || arg == "--reset") resetStats = true;
        else if (arg == "-h" || arg == "--help") {
            cout << "Usage: movie_emoji [easy|medium|hard] [-s] [-r]" << endl;
            return 0;
        }
    }
    if (resetStats) {
        string f = getHomeDir() + "/.movie_emoji_stats.json";
        if (fs::exists(f)) fs::remove(f);
        cout << "Статистика сброшена." << endl;
        return 0;
    }
    if (showStats) {
        string f = getHomeDir() + "/.movie_emoji_stats.json";
        ifstream file(f);
        if (file) {
            string content((istreambuf_iterator<char>(file)), istreambuf_iterator<char>());
            auto extract = [&](const string& key) -> int {
                size_t pos = content.find("\"" + key + "\"");
                if (pos == string::npos) return 0;
                pos = content.find(":", pos) + 1;
                size_t end = content.find(",", pos);
                if (end == string::npos) end = content.find("}", pos);
                try { return stoi(content.substr(pos, end-pos)); } catch (...) { return 0; }
            };
            int games = extract("games"), best = extract("best_score"), movies = extract("total_movies");
            cout << colorize("📊 Статистика:", BOLD) << endl;
            cout << "  Сыграно игр: " << games << endl;
            cout << "  Лучший счёт: " << best << endl;
            cout << "  Всего угадано фильмов: " << movies << endl;
        } else {
            cout << "Статистика пуста." << endl;
        }
        return 0;
    }
    MovieEmojiGame game(level);
    game.play();
    return 0;
}
