package utils

import (
	"log"
	"os"
)

// Уровни логирования
const (
	LevelDebug = iota
	LevelInfo
	LevelWarning
	LevelError
	LevelFatal
)

// Цветовые коды для логирования
const (
	Reset  = "\033[0m"
	Red    = "\033[31m"
	Green  = "\033[32m"
	Yellow = "\033[33m"
	White  = "\033[37m"
)

// Logger оборачивает стандартный логгер и добавляет методы для уровней логирования
type Logger struct {
	logger *log.Logger
	level  int
}

// NewCustomLogger создает новый экземпляр Logger с заданным уровнем логирования
func NewCustomLogger(level int) *Logger {
	return &Logger{
		logger: log.New(os.Stdout, "", log.Ldate|log.Ltime|log.Lshortfile),
		level:  level,
	}
}

// SetLevel позволяет изменить уровень логирования
func (l *Logger) SetLevel(level int) {
	l.level = level
}

// Info логирует сообщение уровня INFO с зеленым цветом
func (l *Logger) Info(msg string) {
	if l.level <= LevelInfo {
		l.logger.Println(Green + "INFO: " + msg + Reset)
	}
}

// Warning логирует сообщение уровня WARNING с желтым цветом
func (l *Logger) Warning(msg string) {
	if l.level <= LevelWarning {
		l.logger.Println(Yellow + "WARNING: " + msg + Reset)
	}
}

// Error логирует сообщение уровня ERROR с красным цветом
func (l *Logger) Error(msg string) {
	if l.level <= LevelError {
		l.logger.Println(Red + "ERROR: " + msg + Reset)
	}
}

// Debug логирует сообщение уровня DEBUG с белым цветом
func (l *Logger) Debug(msg string) {
	if l.level <= LevelDebug {
		l.logger.Println(White + "DEBUG: " + msg + Reset)
	}
}

// Fatal логирует сообщение уровня FATAL с красным цветом и завершает программу
func (l *Logger) Fatal(msg string) {
	if l.level <= LevelFatal {
		l.logger.Fatalln(Red + "FATAL: " + msg + Reset)
	}
}
