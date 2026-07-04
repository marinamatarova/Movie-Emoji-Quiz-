#!/usr/bin/env ruby
# movie_emoji.rb
# encoding: UTF-8

require 'json'
require 'fileutils'
require 'timeout'

COLORS = {
  reset: "\e[0m",
  green: "\e[92m",
  red: "\e[91m",
  yellow: "\e[93m",
  blue: "\e[94m",
  cyan: "\e[96m",
  bold: "\e[1m"
}

def colorize(text, color)
  "#{COLORS[color]}#{text}#{COLORS[:reset]}"
end

MOVIES = {
  'easy' => [
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
  'medium' => [
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
  'hard' => [
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
}

class MovieEmojiGame
  attr_reader :level, :movies, :max_movies, :score, :correct, :skipped,
              :hint_used, :time_limit, :stats, :stats_file

  def initialize(level = 'easy')
    @level = level
    @movies = MOVIES[level]
    @max_movies = 10
    @score = 0
    @correct = 0
    @skipped = 0
    @hint_used = 0
    @stats_file = File.join(Dir.home, '.movie_emoji_stats.json')
    load_stats
    @time_limit = { 'easy' => 60, 'medium' => 45, 'hard' => 30 }[level]
  end

  def load_stats
    if File.exist?(@stats_file)
      @stats = JSON.parse(File.read(@stats_file))
    else
      @stats = { 'games' => 0, 'best_score' => 0, 'total_movies' => 0 }
    end
  end

  def save_stats
    File.write(@stats_file, JSON.pretty_generate(@stats))
  end

  def timer_input(prompt, timeout)
    print colorize(prompt, :bold)
    answer = nil
    begin
      Timeout.timeout(timeout) do
        answer = STDIN.gets.chomp
      end
    rescue Timeout::Error
      answer = nil
    end
    answer
  end

  def show_hint(movie)
    puts colorize("Подсказка: первая буква '#{movie[:title][0]}', всего #{movie[:title].length} букв", :green)
  end

  def play_round(movie)
    puts colorize("\n🎬 Эмодзи: #{movie[:emoji]}", :cyan)
    puts colorize("Подсказка: #{movie[:hint]}", :blue)
    puts 'Введите название фильма, ? для подсказки, pass для пропуска, quit для выхода.'

    start = Time.now
    answer = timer_input('Ваш ответ: ', @time_limit)
    elapsed = Time.now - start

    if answer.nil?
      puts colorize('⏰ Время вышло!', :red)
      @skipped += 1
      puts colorize("Загаданный фильм: #{movie[:title]}", :yellow)
      return
    end
    if answer == 'quit'
      puts 'Выход.'
      save_stats
      exit
    end
    if answer == '?'
      if @hint_used < 1
        show_hint(movie)
        @hint_used += 1
        @score = [0, @score - 5].max
        play_round(movie)
        return
      else
        puts colorize('Подсказка уже использована.', :yellow)
        play_round(movie)
        return
      end
    end
    if answer == 'pass'
      @skipped += 1
      puts colorize("Загаданный фильм: #{movie[:title]}", :yellow)
      return
    end
    if answer.casecmp(movie[:title]).zero?
      @correct += 1
      points = [1, (10 - elapsed / 10).to_i].max
      @score += points
      puts colorize("✅ Верно! +#{points} очков. Время: #{elapsed.round(1)} сек", :green)
    else
      @skipped += 1
      puts colorize("❌ Неверно. Загаданный фильм: #{movie[:title]}", :red)
    end
  end

  def play
    puts colorize('🎬 Добро пожаловать в игру "Угадай фильм по эмодзи"!', :bold)
    puts "Уровень: #{@level}, лимит времени: #{@time_limit} сек."
    puts 'Цель: угадать фильм по набору эмодзи.'
    puts 'Вводите название, используйте ? для подсказки, pass для пропуска, quit для выхода.'

    @movies.shuffle!
    total = [@max_movies, @movies.size].min
    (0...total).each do |i|
      puts colorize("\nФильм #{i+1}/#{total}", :blue)
      play_round(@movies[i])
    end
    puts colorize("\n🏁 Игра завершена!", :bold)
    puts "  Угадано фильмов: #{@correct}"
    puts "  Пропущено: #{@skipped}"
    puts "  Использовано подсказок: #{@hint_used}"
    puts "  Счёт: #{@score}"
    @stats['games'] += 1
    if @score > @stats['best_score']
      @stats['best_score'] = @score
      puts colorize('🏆 Новый рекорд!', :green)
    end
    @stats['total_movies'] += @correct
    save_stats
    puts colorize("Лучший результат: #{@stats['best_score']}", :yellow)
  end
end

def main
  level = 'easy'
  show_stats = false
  reset_stats = false
  ARGV.each do |arg|
    case arg
    when 'easy', 'medium', 'hard'
      level = arg
    when '-s', '--stats'
      show_stats = true
    when '-r', '--reset'
      reset_stats = true
    when '-h', '--help'
      puts 'Usage: ruby movie_emoji.rb [easy|medium|hard] [-s] [-r]'
      return
    end
  end
  if reset_stats
    f = File.join(Dir.home, '.movie_emoji_stats.json')
    File.delete(f) if File.exist?(f)
    puts 'Статистика сброшена.'
    return
  end
  if show_stats
    f = File.join(Dir.home, '.movie_emoji_stats.json')
    if File.exist?(f)
      stats = JSON.parse(File.read(f))
      puts colorize('📊 Статистика:', :bold)
      puts "  Сыграно игр: #{stats['games']}"
      puts "  Лучший счёт: #{stats['best_score']}"
      puts "  Всего угадано фильмов: #{stats['total_movies']}"
    else
      puts 'Статистика пуста.'
    end
    return
  end
  game = MovieEmojiGame.new(level)
  game.play
end

main if __FILE__ == $0
