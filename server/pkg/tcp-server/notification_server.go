package tcp_server

import (
	"fmt"
	"net"
	"sync"

	"google.golang.org/protobuf/proto"
)

// NotificationServer отвечает за управление уведомлениями.
type NotificationServer struct {
	address string
	clients map[net.Conn]struct{}
	mu      sync.Mutex
	logger  ILogger
}

// NewNotificationServer создаёт новый сервер уведомлений.
func NewNotificationServer(address string, logger ILogger) *NotificationServer {
	return &NotificationServer{
		address: address,
		clients: make(map[net.Conn]struct{}),
		logger:  logger,
	}
}

// Start запускает сервер уведомлений.
func (n *NotificationServer) Start() error {
	listener, err := net.Listen("tcp", n.address)
	if err != nil {
		return err
	}
	defer listener.Close()

	n.logger.Info(fmt.Sprintf("Notification server is listening on: %s", n.address))

	for {
		conn, err := listener.Accept()
		if err != nil {
			n.logger.Error(fmt.Sprintf("Failed to accept connection: %v", err))
			continue
		}

		n.mu.Lock()
		n.clients[conn] = struct{}{}
		n.mu.Unlock()

		n.logger.Info(fmt.Sprintf("New notification client connected: %s", conn.RemoteAddr().String()))

		go n.handleConnection(conn)
	}
}

// handleConnection обрабатывает отключение клиента.
func (n *NotificationServer) handleConnection(conn net.Conn) {
	defer func() {
		conn.Close()
		n.mu.Lock()
		delete(n.clients, conn)
		n.mu.Unlock()
		n.logger.Info(fmt.Sprintf("Notification client disconnected: %s", conn.RemoteAddr().String()))
	}()

	// Чтение здесь для того чтобы конекшн просто не падал
	buffer := make([]byte, 1024)
	for {
		m, err := conn.Read(buffer)
		if err != nil {
			n.logger.Error(fmt.Sprintf("Error reading from client %s: %v", conn.RemoteAddr().String(), err))
			conn.Close()
			return
		}
		// Обработка полученных данных
		n.logger.Info(fmt.Sprintf("Received data from %s: %s", conn.RemoteAddr().String(), string(buffer[:m])))
	}
}

// Notify отправляет уведомление только указанным клиентам по IP.
func (n *NotificationServer) Notify(event string, payload []byte, clientIPs []string) {
	n.mu.Lock()
	defer n.mu.Unlock()

	serverResp := &ServerResponse{
		StatusCode: StatusSuccess,
		Message:    event,
		Payload:    payload,
	}

	respBytes, err := proto.Marshal(serverResp)
	if err != nil {
		n.logger.Error(fmt.Sprintf("Failed to serialize notification: %v", err))
		return
	}

	// Создаём множество IP-адресов для быстрого поиска
	targetIPs := make(map[string]struct{}, len(clientIPs))
	for _, ip := range clientIPs {
		targetIPs[ip] = struct{}{}
	}

	// Отправляем только клиентам с указанными IP
	for conn := range n.clients {
		clientIP, _, err := net.SplitHostPort(conn.RemoteAddr().String())
		if err != nil {
			n.logger.Error(fmt.Sprintf("Failed to parse client address: %v", err))
			continue
		}

		// Проверяем, есть ли клиентский IP в списке
		if _, exists := targetIPs[clientIP]; exists {
			if err := writeMessage(conn, respBytes); err != nil {
				n.logger.Error(fmt.Sprintf("Failed to send notification to %s: %v", conn.RemoteAddr().String(), err))
				delete(n.clients, conn)
				conn.Close()
			} else {
				n.logger.Info(fmt.Sprintf("Notification sent to %s: %s", conn.RemoteAddr().String(), serverResp.Message))
			}
		} else {
			n.logger.Debug(fmt.Sprintf("Skipping notification for %s", conn.RemoteAddr().String()))
		}
	}
}
