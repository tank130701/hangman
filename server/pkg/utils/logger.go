package utils

import (
	"log"
	"os"
)

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
}

// NewCustomLogger создает новый экземпляр Logger
func NewCustomLogger() *Logger {
	return &Logger{
		logger: log.New(os.Stdout, "", log.Ldate|log.Ltime|log.Lshortfile),
	}
}

// Info логирует сообщение уровня INFO с зеленым цветом
func (l *Logger) Info(msg string) {
	l.logger.Println(Green + "INFO: " + msg + Reset)
}

// Warning логирует сообщение уровня WARNING с желтым цветом
func (l *Logger) Warning(msg string) {
	l.logger.Println(Yellow + "WARNING: " + msg + Reset)
}

// Error логирует сообщение уровня ERROR с красным цветом
func (l *Logger) Error(msg string) {
	l.logger.Println(Red + "ERROR: " + msg + Reset)
}

// Debug логирует сообщение уровня DEBUG с белым цветом
func (l *Logger) Debug(msg string) {
	l.logger.Println(White + "DEBUG: " + msg + Reset)
}
