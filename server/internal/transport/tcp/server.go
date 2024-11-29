package tcp

import (
	"bufio"
	"log"
	"net"
	"strings"
)

type Server struct {
	address string
	handler *Handler

	logger *log.Logger
}

// New создает новый сервер
func New(address string, handler *Handler, logger *log.Logger) *Server {
	return &Server{
		address: address,
		handler: handler,
		logger:  logger,
	}
}

// Start запускает сервер и обрабатывает подключения
func (s *Server) Start() error {
	listener, err := net.Listen("tcp", s.address)
	if err != nil {
		return err
	}
	defer listener.Close()

	s.logger.Println("Server is listening on", s.address)
	for {
		conn, err := listener.Accept()
		if err != nil {
			s.logger.Println("Failed to accept connection:", err)
			continue
		}

		go s.handleConnection(conn)
	}
}

// handleConnection обрабатывает подключение клиента
func (s *Server) handleConnection(conn net.Conn) {
	defer conn.Close()
	clientAddr := conn.RemoteAddr().String()
	s.logger.Printf("New connection from %s", clientAddr)

	reader := bufio.NewReader(conn)

	for {
		var response []byte
		// conn.Write([]byte("Enter a JSON command:\n"))
		message, err := reader.ReadString('\n')
		if err != nil {
			response = createErrorResponse(ErrCodeInternalServerError, err.Error())
		} else {
			response = s.processMessage([]byte(strings.TrimSpace(message)), conn)
		}
		conn.Write(response)
		conn.Write([]byte("\n"))
	}
}
