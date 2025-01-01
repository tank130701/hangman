package tcp_server

import (
	"context"
	"net"
)

// Определяем тип для ключей контекста
type ctxKey string

func (c ctxKey) String() string {
	return "ctx_key-" + string(c)
}

const (
	ConnKey               ctxKey = "conn"
	LoggerKey             ctxKey = "logger"
	CancelKey             ctxKey = "cancel"
	NotificationServerKey ctxKey = "notificationServer"
)

// SetConn устанавливает соединение в контекст.
func SetConn(ctx context.Context, conn net.Conn) context.Context {
	return context.WithValue(ctx, ConnKey, conn)
}

// GetConn извлекает соединение из контекста.
func GetConn(ctx context.Context) (net.Conn, bool) {
	conn, ok := ctx.Value(ConnKey).(net.Conn)
	return conn, ok
}

// SetLogger устанавливает логгер в контекст.
func SetLogger(ctx context.Context, logger ILogger) context.Context {
	return context.WithValue(ctx, LoggerKey, logger)
}

// GetLogger извлекает логгер из контекста.
func GetLogger(ctx context.Context) (ILogger, bool) {
	logger, ok := ctx.Value(LoggerKey).(ILogger)
	return logger, ok
}

// SetCancel устанавливает функцию отмены в контекст.
func SetCancel(ctx context.Context, cancel context.CancelFunc) context.Context {
	return context.WithValue(ctx, CancelKey, cancel)
}

// GetCancel извлекает функцию отмены из контекста.
func GetCancel(ctx context.Context) (context.CancelFunc, bool) {
	cancel, ok := ctx.Value(CancelKey).(context.CancelFunc)
	return cancel, ok
}

// SetNotificationServer устанавливает сервер уведомлений в контекст.
func SetNotificationServer(ctx context.Context, notificationServer *NotificationServer) context.Context {
	return context.WithValue(ctx, NotificationServerKey, notificationServer)
}

// GetNotificationServer извлекает сервер уведомлений из контекста.
func GetNotificationServer(ctx context.Context) (*NotificationServer, bool) {
	notificationServer, ok := ctx.Value(NotificationServerKey).(*NotificationServer)
	return notificationServer, ok
}
