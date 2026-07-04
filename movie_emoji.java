// movie_emoji.java
import java.io.*;
import java.nio.file.*;
import java.util.*;
import java.util.concurrent.*;

public class movie_emoji {
    private static final String RESET = "\u001B[0m";
    private static final String GREEN = "\u001B[92m";
    private static final String RED = "\u001B[91m";
    private static final String YELLOW = "\u001B[93m";
    private static final String BLUE = "\u001B[94m";
    private static final String CYAN = "\u001B[96m";
    private static final String BOLD = "\u001B[1m";

    private static String colorize(String text, String color) {
        return color + text + RESET;
    }

    private static class Movie {
        String emoji, title, hint;
        Movie(String e, String t, String h) { emoji=e; title=t; hint=h; }
    }

    private static final Map<String, List<Movie>> MOVIES = new HashMap<>();
    static {
        MOVIES.put("easy", Arrays.asList(
            new Movie("🦁👑", "Король Лев", "Мультфильм Диснея"),
            new Movie("🚀🌌", "Звёздные войны", "Космическая сага"),
            new Movie("🧙‍♂️💍", "Властелин колец", "Фэнтези-трилогия"),
            new Movie("🦇🃏", "Тёмный рыцарь", "Фильм о Бэтмене"),
            new Movie("🤖🔫", "Терминатор", "Культовый фантастический боевик"),
            new Movie("🦈🌊", "Челюсти", "Фильм ужасов о большой белой акуле"),
            new Movie("👽👾", "Инопланетянин", "Стивен Спилберг"),
            new Movie("🚢💖", "Титаник", "Романтическая драма"),
            new Movie("🐉⚔️", "Гарри Поттер", "Фильм о волшебнике"),
            new Movie("🧟‍♂️🧠", "Зомбиленд", "Кинокомедия про зомби")
        ));
        MOVIES.put("medium", Arrays.asList(
            new Movie("🔫🐟", "Крепкий орешек", "Боевик с Брюсом Уиллисом"),
            new Movie("👻🏠", "Оно", "Фильм ужасов о клоуне"),
            new Movie("🧜‍♀️🧽", "Губка Боб", "Мультфильм о морских обитателях"),
            new Movie("🐺📈", "Волк с Уолл-стрит", "Драма о финансисте"),
            new Movie("🌪️🏠", "Унесённые ветром", "Классическая мелодрама"),
            new Movie("🐧🎬", "Мадагаскар", "Мультфильм о животных"),
            new Movie("🤡🎈", "Оно", "Фильм ужасов"),
            new Movie("🧛‍♂️🌙", "Дракула", "Готический фильм ужасов"),
            new Movie("🚗💨", "Форсаж", "Скоростной боевик"),
            new Movie("🔪😱", "Пила", "Фильм ужасов")
        ));
        MOVIES.put("hard", Arrays.asList(
            new Movie("🦑🐟", "В поисках Немо", "Мультфильм о рыбках"),
            new Movie("🐭🍝", "Рататуй", "Мультфильм о крысе-поваре"),
            new Movie("🎪🎭", "Безумный Макс", "Постапокалиптический боевик"),
            new Movie("🚲🎬", "Велосипедист", "Драма о гонщике"),
            new Movie("🐺📖", "Американский психопат", "Психологический триллер"),
            new Movie("👨‍🦳🧠", "Игры разума", "Драма о гениальном математике"),
            new Movie("🕵️‍♂️🔍", "Шерлок Холмс", "Детективный фильм"),
            new Movie("🧛‍♂️❤️", "Сумерки", "Фильм о вампирах"),
            new Movie("🦍🏢", "Кинг-Конг", "Приключенческий фильм"),
            new Movie("👽🌍", "День независимости", "Фантастический фильм")
        ));
    }

    private static class Stats {
        int games, best_score, total_movies;
    }

    private String level;
    private List<Movie> movies;
    private int maxMovies;
    private int score;
    private int correct;
    private int skipped;
    private int hintUsed;
    private int timeLimit;
    private Stats stats;
    private String statsFile;
    private Scanner scanner;

    public movie_emoji(String lvl) {
        level = lvl;
        movies = MOVIES.get(level);
        maxMovies = 10;
        score = 0;
        correct = 0;
        skipped = 0;
        hintUsed = 0;
        statsFile = System.getProperty("user.home") + "/.movie_emoji_stats.json";
        loadStats();
        timeLimit = level.equals("easy") ? 60 : level.equals("medium") ? 45 : 30;
        scanner = new Scanner(System.in);
    }

    private void loadStats() {
        stats = new Stats();
        try {
            String json = new String(Files.readAllBytes(Paths.get(statsFile)));
            stats.games = extractInt(json, "games");
            stats.best_score = extractInt(json, "best_score");
            stats.total_movies = extractInt(json, "total_movies");
        } catch (Exception e) {}
    }

    private int extractInt(String json, String key) {
        int idx = json.indexOf("\"" + key + "\"");
        if (idx == -1) return 0;
        int start = json.indexOf(":", idx) + 1;
        int end = json.indexOf(",", start);
        if (end == -1) end = json.indexOf("}", start);
        try { return Integer.parseInt(json.substring(start, end).trim()); } catch (Exception e) { return 0; }
    }

    private void saveStats() {
        try {
            String json = "{\"games\":" + stats.games + ",\"best_score\":" + stats.best_score +
                          ",\"total_movies\":" + stats.total_movies + "}";
            Files.write(Paths.get(statsFile), json.getBytes());
        } catch (IOException e) {}
    }

    private String timerInput(String prompt, int timeout) {
        System.out.print(colorize(prompt, BOLD));
        ExecutorService executor = Executors.newSingleThreadExecutor();
        Future<String> future = executor.submit(() -> scanner.nextLine());
        try {
            return future.get(timeout, TimeUnit.SECONDS);
        } catch (TimeoutException e) {
            future.cancel(true);
            return "";
        } catch (Exception e) {
            return "";
        } finally {
            executor.shutdownNow();
        }
    }

    private void showHint(Movie movie) {
        System.out.println(colorize("Подсказка: первая буква '" + movie.title.charAt(0) + "', всего " + movie.title.length() + " букв", GREEN));
    }

    private void playRound(Movie movie) {
        System.out.println(colorize("\n🎬 Эмодзи: " + movie.emoji, CYAN));
        System.out.println(colorize("Подсказка: " + movie.hint, BLUE));
        System.out.println("Введите название фильма, ? для подсказки, pass для пропуска, quit для выхода.");

        long start = System.currentTimeMillis();
        String answer = timerInput("Ваш ответ: ", timeLimit);
        double elapsed = (System.currentTimeMillis() - start) / 1000.0;

        if (answer.isEmpty()) {
            System.out.println(colorize("⏰ Время вышло!", RED));
            skipped++;
            System.out.println(colorize("Загаданный фильм: " + movie.title, YELLOW));
            return;
        }
        if (answer.equals("quit")) {
            System.out.println("Выход.");
            saveStats();
            System.exit(0);
        }
        if (answer.equals("?")) {
            if (hintUsed < 1) {
                showHint(movie);
                hintUsed++;
                score = Math.max(0, score - 5);
                playRound(movie);
                return;
            } else {
                System.out.println(colorize("Подсказка уже использована.", YELLOW));
                playRound(movie);
                return;
            }
        }
        if (answer.equals("pass")) {
            skipped++;
            System.out.println(colorize("Загаданный фильм: " + movie.title, YELLOW));
            return;
        }
        if (answer.equalsIgnoreCase(movie.title)) {
            correct++;
            int points = Math.max(1, (int)(10 - elapsed / 10));
            score += points;
            System.out.println(colorize("✅ Верно! +" + points + " очков. Время: " + elapsed + " сек", GREEN));
        } else {
            skipped++;
            System.out.println(colorize("❌ Неверно. Загаданный фильм: " + movie.title, RED));
        }
    }

    public void play() {
        System.out.println(colorize("🎬 Добро пожаловать в игру 'Угадай фильм по эмодзи'!", BOLD));
        System.out.println("Уровень: " + level + ", лимит времени: " + timeLimit + " сек.");
        System.out.println("Цель: угадать фильм по набору эмодзи.");
        System.out.println("Вводите название, используйте ? для подсказки, pass для пропуска, quit для выхода.\n");

        Collections.shuffle(movies);
        int total = Math.min(maxMovies, movies.size());
        for (int i = 0; i < total; i++) {
            System.out.println(colorize("\nФильм " + (i+1) + "/" + total, BLUE));
            playRound(movies.get(i));
        }
        System.out.println(colorize("\n🏁 Игра завершена!", BOLD));
        System.out.println("  Угадано фильмов: " + correct);
        System.out.println("  Пропущено: " + skipped);
        System.out.println("  Использовано подсказок: " + hintUsed);
        System.out.println("  Счёт: " + score);
        stats.games++;
        if (score > stats.best_score) {
            stats.best_score = score;
            System.out.println(colorize("🏆 Новый рекорд!", GREEN));
        }
        stats.total_movies += correct;
        saveStats();
        System.out.println(colorize("Лучший результат: " + stats.best_score, YELLOW));
        scanner.close();
    }

    public static void main(String[] args) {
        String level = "easy";
        boolean showStats = false, resetStats = false;
        for (String arg : args) {
            if (arg.equals("easy") || arg.equals("medium") || arg.equals("hard")) level = arg;
            else if (arg.equals("-s") || arg.equals("--stats")) showStats = true;
            else if (arg.equals("-r") || arg.equals("--reset")) resetStats = true;
            else if (arg.equals("-h") || arg.equals("--help")) {
                System.out.println("Usage: java movie_emoji [easy|medium|hard] [-s] [-r]");
                return;
            }
        }
        if (resetStats) {
            String f = System.getProperty("user.home") + "/.movie_emoji_stats.json";
            try { Files.deleteIfExists(Paths.get(f)); } catch (Exception e) {}
            System.out.println("Статистика сброшена.");
            return;
        }
        if (showStats) {
            String f = System.getProperty("user.home") + "/.movie_emoji_stats.json";
            try {
                String json = new String(Files.readAllBytes(Paths.get(f)));
                int games = extractInt(json, "games");
                int best = extractInt(json, "best_score");
                int movies = extractInt(json, "total_movies");
                System.out.println(colorize("📊 Статистика:", BOLD));
                System.out.println("  Сыграно игр: " + games);
                System.out.println("  Лучший счёт: " + best);
                System.out.println("  Всего угадано фильмов: " + movies);
            } catch (Exception e) {
                System.out.println("Статистика пуста.");
            }
            return;
        }
        movie_emoji game = new movie_emoji(level);
        game.play();
    }

    private static int extractInt(String json, String key) {
        int idx = json.indexOf("\"" + key + "\"");
        if (idx == -1) return 0;
        int start = json.indexOf(":", idx) + 1;
        int end = json.indexOf(",", start);
        if (end == -1) end = json.indexOf("}", start);
        try { return Integer.parseInt(json.substring(start, end).trim()); } catch (Exception e) { return 0; }
    }
}
