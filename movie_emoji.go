// movie_emoji.go
package main

import (
	"bufio"
	"encoding/json"
	"fmt"
	"math/rand"
	"os"
	"path/filepath"
	"strings"
	"time"
)

const (
	reset  = "\033[0m"
	green  = "\033[92m"
	red    = "\033[91m"
	yellow = "\033[93m"
	blue   = "\033[94m"
	cyan   = "\033[96m"
	bold   = "\033[1m"
)

func colorize(text, color string) string {
	return color + text + reset
}

type Movie struct {
	Emoji string
	Title string
	Hint  string
}

var moviesMap = map[string][]Movie{
	"easy": {
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
	},
	"medium": {
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
	},
	"hard": {
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
	},
}

type Stats struct {
	Games      int `json:"games"`
	BestScore  int `json:"best_score"`
	TotalMovies int `json:"total_movies"`
}

type MovieEmojiGame struct {
	level     string
	movies    []Movie
	maxMovies int
	score     int
	correct   int
	skipped   int
	hintUsed  int
	timeLimit int
	stats     Stats
	statsFile string
}

func NewMovieEmojiGame(level string) *MovieEmojiGame {
	g := &MovieEmojiGame{
		level:     level,
		maxMovies: 10,
		statsFile: filepath.Join(os.Getenv("HOME"), ".movie_emoji_stats.json"),
	}
	g.movies = moviesMap[level]
	g.loadStats()
	switch level {
	case "easy":
		g.timeLimit = 60
	case "medium":
		g.timeLimit = 45
	default:
		g.timeLimit = 30
	}
	return g
}

func (g *MovieEmojiGame) loadStats() {
	data, err := os.ReadFile(g.statsFile)
	if err != nil {
		g.stats = Stats{}
		return
	}
	json.Unmarshal(data, &g.stats)
}

func (g *MovieEmojiGame) saveStats() {
	data, _ := json.MarshalIndent(g.stats, "", "  ")
	os.WriteFile(g.statsFile, data, 0644)
}

func (g *MovieEmojiGame) timerInput(prompt string, timeout int) string {
	fmt.Print(colorize(prompt, bold))
	ch := make(chan string)
	go func() {
		scanner := bufio.NewScanner(os.Stdin)
		if scanner.Scan() {
			ch <- scanner.Text()
		}
	}()
	select {
	case answer := <-ch:
		return answer
	case <-time.After(time.Duration(timeout) * time.Second):
		return ""
	}
}

func (g *MovieEmojiGame) showHint(movie Movie) {
	fmt.Printf("%s\n", colorize(fmt.Sprintf("Подсказка: первая буква '%c', всего %d букв", movie.Title[0], len(movie.Title)), green))
}

func (g *MovieEmojiGame) playRound(movie Movie) {
	fmt.Printf("%s\n", colorize("\n🎬 Эмодзи: "+movie.Emoji, cyan))
	fmt.Printf("%s\n", colorize("Подсказка: "+movie.Hint, blue))
	fmt.Println("Введите название фильма, ? для подсказки, pass для пропуска, quit для выхода.")

	start := time.Now()
	answer := g.timerInput("Ваш ответ: ", g.timeLimit)
	elapsed := time.Since(start).Seconds()

	if answer == "" {
		fmt.Println(colorize("⏰ Время вышло!", red))
		g.skipped++
		fmt.Printf("%s\n", colorize("Загаданный фильм: "+movie.Title, yellow))
		return
	}
	if answer == "quit" {
		fmt.Println("Выход.")
		g.saveStats()
		os.Exit(0)
	}
	if answer == "?" {
		if g.hintUsed < 1 {
			g.showHint(movie)
			g.hintUsed++
			g.score = max(0, g.score-5)
			g.playRound(movie)
			return
		} else {
			fmt.Println(colorize("Подсказка уже использована.", yellow))
			g.playRound(movie)
			return
		}
	}
	if answer == "pass" {
		g.skipped++
		fmt.Printf("%s\n", colorize("Загаданный фильм: "+movie.Title, yellow))
		return
	}
	if strings.EqualFold(answer, movie.Title) {
		g.correct++
		points := int(max(1, 10-elapsed/10))
		g.score += points
		fmt.Printf("%s\n", colorize(fmt.Sprintf("✅ Верно! +%d очков. Время: %.1f сек", points, elapsed), green))
	} else {
		g.skipped++
		fmt.Printf("%s\n", colorize("❌ Неверно. Загаданный фильм: "+movie.Title, red))
	}
}

func max(a, b int) int {
	if a > b {
		return a
	}
	return b
}

func (g *MovieEmojiGame) play() {
	fmt.Println(colorize("🎬 Добро пожаловать в игру 'Угадай фильм по эмодзи'!", bold))
	fmt.Printf("Уровень: %s, лимит времени: %d сек.\n", g.level, g.timeLimit)
	fmt.Println("Цель: угадать фильм по набору эмодзи.")
	fmt.Println("Вводите название, используйте ? для подсказки, pass для пропуска, quit для выхода.\n")

	rand.Seed(time.Now().UnixNano())
	rand.Shuffle(len(g.movies), func(i, j int) {
		g.movies[i], g.movies[j] = g.movies[j], g.movies[i]
	})
	total := min(g.maxMovies, len(g.movies))
	for i := 0; i < total; i++ {
		fmt.Printf("%s\n", colorize(fmt.Sprintf("\nФильм %d/%d", i+1, total), blue))
		g.playRound(g.movies[i])
	}
	fmt.Println(colorize("\n🏁 Игра завершена!", bold))
	fmt.Printf("  Угадано фильмов: %d\n", g.correct)
	fmt.Printf("  Пропущено: %d\n", g.skipped)
	fmt.Printf("  Использовано подсказок: %d\n", g.hintUsed)
	fmt.Printf("  Счёт: %d\n", g.score)
	g.stats.Games++
	if g.score > g.stats.BestScore {
		g.stats.BestScore = g.score
		fmt.Println(colorize("🏆 Новый рекорд!", green))
	}
	g.stats.TotalMovies += g.correct
	g.saveStats()
	fmt.Printf("%s\n", colorize(fmt.Sprintf("Лучший результат: %d", g.stats.BestScore), yellow))
}

func min(a, b int) int {
	if a < b {
		return a
	}
	return b
}

func main() {
	level := "easy"
	showStats := false
	resetStats := false
	args := os.Args[1:]
	for i := 0; i < len(args); i++ {
		arg := args[i]
		switch arg {
		case "easy", "medium", "hard":
			level = arg
		case "-s", "--stats":
			showStats = true
		case "-r", "--reset":
			resetStats = true
		case "-h", "--help":
			fmt.Println("Usage: movie_emoji [easy|medium|hard] [-s] [-r]")
			return
		}
	}
	if resetStats {
		f := filepath.Join(os.Getenv("HOME"), ".movie_emoji_stats.json")
		os.Remove(f)
		fmt.Println("Статистика сброшена.")
		return
	}
	if showStats {
		f := filepath.Join(os.Getenv("HOME"), ".movie_emoji_stats.json")
		data, err := os.ReadFile(f)
		if err != nil {
			fmt.Println("Статистика пуста.")
			return
		}
		var stats Stats
		json.Unmarshal(data, &stats)
		fmt.Println(colorize("📊 Статистика:", bold))
		fmt.Printf("  Сыграно игр: %d\n", stats.Games)
		fmt.Printf("  Лучший счёт: %d\n", stats.BestScore)
		fmt.Printf("  Всего угадано фильмов: %d\n", stats.TotalMovies)
		return
	}
	game := NewMovieEmojiGame(level)
	game.play()
}
