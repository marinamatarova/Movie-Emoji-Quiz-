// movie_emoji.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Diagnostics;

class MovieEmojiGame
{
    static string Colorize(string text, string color)
    {
        string col = color switch
        {
            "green" => "\x1b[92m",
            "red" => "\x1b[91m",
            "yellow" => "\x1b[93m",
            "blue" => "\x1b[94m",
            "cyan" => "\x1b[96m",
            "bold" => "\x1b[1m",
            _ => "\x1b[0m"
        };
        return col + text + "\x1b[0m";
    }

    class Movie
    {
        public string Emoji { get; set; }
        public string Title { get; set; }
        public string Hint { get; set; }
    }

    static Dictionary<string, List<Movie>> MOVIES = new Dictionary<string, List<Movie>>
    {
        {"easy", new List<Movie>{
            new Movie{Emoji="🦁👑", Title="Король Лев", Hint="Мультфильм Диснея"},
            new Movie{Emoji="🚀🌌", Title="Звёздные войны", Hint="Космическая сага"},
            new Movie{Emoji="🧙‍♂️💍", Title="Властелин колец", Hint="Фэнтези-трилогия"},
            new Movie{Emoji="🦇🃏", Title="Тёмный рыцарь", Hint="Фильм о Бэтмене"},
            new Movie{Emoji="🤖🔫", Title="Терминатор", Hint="Культовый фантастический боевик"},
            new Movie{Emoji="🦈🌊", Title="Челюсти", Hint="Фильм ужасов о большой белой акуле"},
            new Movie{Emoji="👽👾", Title="Инопланетянин", Hint="Стивен Спилберг"},
            new Movie{Emoji="🚢💖", Title="Титаник", Hint="Романтическая драма"},
            new Movie{Emoji="🐉⚔️", Title="Гарри Поттер", Hint="Фильм о волшебнике"},
            new Movie{Emoji="🧟‍♂️🧠", Title="Зомбиленд", Hint="Кинокомедия про зомби"},
        }},
        {"medium", new List<Movie>{
            new Movie{Emoji="🔫🐟", Title="Крепкий орешек", Hint="Боевик с Брюсом Уиллисом"},
            new Movie{Emoji="👻🏠", Title="Оно", Hint="Фильм ужасов о клоуне"},
            new Movie{Emoji="🧜‍♀️🧽", Title="Губка Боб", Hint="Мультфильм о морских обитателях"},
            new Movie{Emoji="🐺📈", Title="Волк с Уолл-стрит", Hint="Драма о финансисте"},
            new Movie{Emoji="🌪️🏠", Title="Унесённые ветром", Hint="Классическая мелодрама"},
            new Movie{Emoji="🐧🎬", Title="Мадагаскар", Hint="Мультфильм о животных"},
            new Movie{Emoji="🤡🎈", Title="Оно", Hint="Фильм ужасов"},
            new Movie{Emoji="🧛‍♂️🌙", Title="Дракула", Hint="Готический фильм ужасов"},
            new Movie{Emoji="🚗💨", Title="Форсаж", Hint="Скоростной боевик"},
            new Movie{Emoji="🔪😱", Title="Пила", Hint="Фильм ужасов"},
        }},
        {"hard", new List<Movie>{
            new Movie{Emoji="🦑🐟", Title="В поисках Немо", Hint="Мультфильм о рыбках"},
            new Movie{Emoji="🐭🍝", Title="Рататуй", Hint="Мультфильм о крысе-поваре"},
            new Movie{Emoji="🎪🎭", Title="Безумный Макс", Hint="Постапокалиптический боевик"},
            new Movie{Emoji="🚲🎬", Title="Велосипедист", Hint="Драма о гонщике"},
            new Movie{Emoji="🐺📖", Title="Американский психопат", Hint="Психологический триллер"},
            new Movie{Emoji="👨‍🦳🧠", Title="Игры разума", Hint="Драма о гениальном математике"},
            new Movie{Emoji="🕵️‍♂️🔍", Title="Шерлок Холмс", Hint="Детективный фильм"},
            new Movie{Emoji="🧛‍♂️❤️", Title="Сумерки", Hint="Фильм о вампирах"},
            new Movie{Emoji="🦍🏢", Title="Кинг-Конг", Hint="Приключенческий фильм"},
            new Movie{Emoji="👽🌍", Title="День независимости", Hint="Фантастический фильм"},
        }}
    };

    class Stats
    {
        public int games { get; set; }
        public int best_score { get; set; }
        public int total_movies { get; set; }
    }

    private string level;
    private List<Movie> movies;
    private int maxMovies;
    private int score;
    private int correct;
    private int skipped;
    private int hintUsed;
    private int timeLimit;
    private Stats stats;
    private string statsFile;

    public MovieEmojiGame(string lvl)
    {
        level = lvl;
        movies = MOVIES[level];
        maxMovies = 10;
        score = 0;
        correct = 0;
        skipped = 0;
        hintUsed = 0;
        statsFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".movie_emoji_stats.json");
        LoadStats();
        timeLimit = level == "easy" ? 60 : level == "medium" ? 45 : 30;
    }

    void LoadStats()
    {
        if (File.Exists(statsFile))
        {
            try
            {
                string json = File.ReadAllText(statsFile);
                stats = JsonSerializer.Deserialize<Stats>(json);
            }
            catch { stats = new Stats(); }
        }
        else stats = new Stats();
    }

    void SaveStats()
    {
        string json = JsonSerializer.Serialize(stats);
        File.WriteAllText(statsFile, json);
    }

    string TimerInput(string prompt, int timeout)
    {
        Console.Write(Colorize(prompt, "bold"));
        var sw = Stopwatch.StartNew();
        string input = "";
        while (sw.Elapsed.TotalSeconds < timeout)
        {
            if (Console.KeyAvailable)
            {
                input = Console.ReadLine();
                return input;
            }
            int remaining = timeout - (int)sw.Elapsed.TotalSeconds;
            Console.Write($"\r{Colorize($"Осталось времени: {remaining} сек", "yellow")}");
            Thread.Sleep(1000);
        }
        return "";
    }

    void ShowHint(Movie movie)
    {
        Console.WriteLine(Colorize($"Подсказка: первая буква '{movie.Title[0]}', всего {movie.Title.Length} букв", "green"));
    }

    void PlayRound(Movie movie)
    {
        Console.WriteLine(Colorize($"\n🎬 Эмодзи: {movie.Emoji}", "cyan"));
        Console.WriteLine(Colorize($"Подсказка: {movie.Hint}", "blue"));
        Console.WriteLine("Введите название фильма, ? для подсказки, pass для пропуска, quit для выхода.");

        var sw = Stopwatch.StartNew();
        string answer = TimerInput("Ваш ответ: ", timeLimit);
        sw.Stop();
        double elapsed = sw.Elapsed.TotalSeconds;

        if (string.IsNullOrEmpty(answer))
        {
            Console.WriteLine(Colorize("⏰ Время вышло!", "red"));
            skipped++;
            Console.WriteLine(Colorize($"Загаданный фильм: {movie.Title}", "yellow"));
            return;
        }
        if (answer == "quit")
        {
            Console.WriteLine("Выход.");
            SaveStats();
            Environment.Exit(0);
        }
        if (answer == "?")
        {
            if (hintUsed < 1)
            {
                ShowHint(movie);
                hintUsed++;
                score = Math.Max(0, score - 5);
                PlayRound(movie);
                return;
            }
            else
            {
                Console.WriteLine(Colorize("Подсказка уже использована.", "yellow"));
                PlayRound(movie);
                return;
            }
        }
        if (answer == "pass")
        {
            skipped++;
            Console.WriteLine(Colorize($"Загаданный фильм: {movie.Title}", "yellow"));
            return;
        }
        if (answer.Equals(movie.Title, StringComparison.OrdinalIgnoreCase))
        {
            correct++;
            int points = Math.Max(1, (int)(10 - elapsed / 10));
            score += points;
            Console.WriteLine(Colorize($"✅ Верно! +{points} очков. Время: {elapsed:F1} сек", "green"));
        }
        else
        {
            skipped++;
            Console.WriteLine(Colorize($"❌ Неверно. Загаданный фильм: {movie.Title}", "red"));
        }
    }

    public void Play()
    {
        Console.WriteLine(Colorize("🎬 Добро пожаловать в игру 'Угадай фильм по эмодзи'!", "bold"));
        Console.WriteLine($"Уровень: {level}, лимит времени: {timeLimit} сек.");
        Console.WriteLine("Цель: угадать фильм по набору эмодзи.");
        Console.WriteLine("Вводите название, используйте ? для подсказки, pass для пропуска, quit для выхода.\n");

        Random rnd = new Random();
        movies = movies.OrderBy(x => rnd.Next()).ToList();
        int total = Math.Min(maxMovies, movies.Count);
        for (int i = 0; i < total; i++)
        {
            Console.WriteLine(Colorize($"\nФильм {i+1}/{total}", "blue"));
            PlayRound(movies[i]);
        }
        Console.WriteLine(Colorize("\n🏁 Игра завершена!", "bold"));
        Console.WriteLine($"  Угадано фильмов: {correct}");
        Console.WriteLine($"  Пропущено: {skipped}");
        Console.WriteLine($"  Использовано подсказок: {hintUsed}");
        Console.WriteLine($"  Счёт: {score}");
        stats.games++;
        if (score > stats.best_score)
        {
            stats.best_score = score;
            Console.WriteLine(Colorize("🏆 Новый рекорд!", "green"));
        }
        stats.total_movies += correct;
        SaveStats();
        Console.WriteLine(Colorize($"Лучший результат: {stats.best_score}", "yellow"));
    }

    static void Main(string[] args)
    {
        string level = "easy";
        bool showStats = false, resetStats = false;
        foreach (var arg in args)
        {
            if (arg == "easy" || arg == "medium" || arg == "hard") level = arg;
            else if (arg == "-s" || arg == "--stats") showStats = true;
            else if (arg == "-r" || arg == "--reset") resetStats = true;
            else if (arg == "-h" || arg == "--help")
            {
                Console.WriteLine("Usage: movie_emoji [easy|medium|hard] [-s] [-r]");
                return;
            }
        }
        if (resetStats)
        {
            string f = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".movie_emoji_stats.json");
            if (File.Exists(f)) File.Delete(f);
            Console.WriteLine("Статистика сброшена.");
            return;
        }
        if (showStats)
        {
            string f = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".movie_emoji_stats.json");
            if (File.Exists(f))
            {
                try
                {
                    string json = File.ReadAllText(f);
                    var stats = JsonSerializer.Deserialize<Stats>(json);
                    Console.WriteLine(Colorize("📊 Статистика:", "bold"));
                    Console.WriteLine($"  Сыграно игр: {stats.games}");
                    Console.WriteLine($"  Лучший счёт: {stats.best_score}");
                    Console.WriteLine($"  Всего угадано фильмов: {stats.total_movies}");
                }
                catch { Console.WriteLine("Статистика пуста."); }
            }
            else Console.WriteLine("Статистика пуста.");
            return;
        }
        MovieEmojiGame game = new MovieEmojiGame(level);
        game.Play();
    }
}
