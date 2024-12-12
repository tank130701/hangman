package tcp_server

import (
	"bufio"
	"encoding/json"
	"log"
	"net"
)

type Server struct {
	address  string
	handlers map[string]HandleFunc

	logger *log.Logger
}

// New создает новый сервер
func New(address string, logger *log.Logger) *Server {
	return &Server{
		address:  address,
		handlers: make(map[string]HandleFunc),
		logger:   logger,
	}
}

// RegisterHandler registers a handler for a specific command.
func (s *Server) RegisterHandler(command string, handler HandleFunc) {
	//s.mu.Lock()
	//defer s.mu.Unlock()
	s.handlers[command] = handler
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
			response = CreateErrorResponse(ErrCodeInternalServerError, err.Error())
		} else {
			response = s.processMessage([]byte(message), conn)
		}
		conn.Write(response)
		conn.Write([]byte("\n"))
	}
}

// processMessage обрабатывает JSON-команды
func (s *Server) processMessage(message []byte, conn net.Conn) []byte {
	// Логируем входящий запрос
	s.logger.Printf("Received message: %s", string(message))

	var request BaseRequest
	if err := json.Unmarshal(message, &request); err != nil {
		errMessage := "Invalid JSON format"
		s.logger.Printf("Error parsing request: %v", err)
		return CreateErrorResponse(4000, errMessage)
	}

	var response []byte

	handler, exists := s.handlers[request.Command]
	if exists {
		response = handler(conn, message)
	} else {
		unknownCommandMessage := "Unknown command"
		s.logger.Printf("No handler for command: %s", request.Command)
		return CreateErrorResponse(ErrCodeUnknownCommand, unknownCommandMessage)
	}

	// Логируем ответ
	var debugData map[string]interface{}
	if err := json.Unmarshal(response, &debugData); err != nil {
		s.logger.Printf("Failed to parse response JSON: %v", err)
	} else {
		prettyResponse, _ := json.MarshalIndent(debugData, "", "  ") // Форматирование JSON для читаемости
		s.logger.Printf("Response JSON:\n%s", prettyResponse)
	}

	return response
}
