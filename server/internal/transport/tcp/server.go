package tcp

import (
	"bufio"
	"encoding/json"
	"errors"
	"hangman/internal/domain"
	"log"
	"net"
	"strings"
)

type Server struct {
	Address        string
	RoomController domain.IRoomController
	Logger         *log.Logger
}

// New создает новый сервер
func New(address string, roomController domain.IRoomController, logger *log.Logger) *Server {
	return &Server{
		Address:        address,
		RoomController: roomController,
		Logger:         logger,
	}
}

// Start запускает сервер и обрабатывает подключения
func (s *Server) Start() error {
	listener, err := net.Listen("tcp", s.Address)
	if err != nil {
		return err
	}
	defer listener.Close()

	s.Logger.Println("Server is listening on", s.Address)
	for {
		conn, err := listener.Accept()
		if err != nil {
			s.Logger.Println("Failed to accept connection:", err)
			continue
		}

		go s.handleConnection(conn)
	}
}

// handleConnection обрабатывает подключение клиента
func (s *Server) handleConnection(conn net.Conn) {
	defer conn.Close()
	clientAddr := conn.RemoteAddr().String()
	s.Logger.Printf("New connection from %s", clientAddr)

	reader := bufio.NewReader(conn)

	// Первое сообщение должно содержать имя пользователя
	player, err := s.registerPlayer(conn, reader)
	if err != nil {
		s.Logger.Printf("Player registration failed: %v", err)
		return
	}

	// Обработка команд игрока
	s.Logger.Printf("Player %s registered successfully", player.Name)
	if err := s.handlePlayerCommands(conn, reader, player); err != nil {
		s.Logger.Printf("Error handling commands for player %s: %v", player.Name, err)
	}
}

// registerPlayer регистрирует игрока
func (s *Server) registerPlayer(conn net.Conn, reader *bufio.Reader) (*domain.Player, error) {
	conn.Write([]byte("Please send your name in JSON format: {\"command\": \"SET_NAME\", \"name\": \"PlayerName\"}\n"))

	message, err := reader.ReadString('\n')
	if err != nil {
		return nil, errors.New("failed to read initial message")
	}

	message = strings.TrimSpace(message)

	var request struct {
		Command string `json:"command"`
		Name    string `json:"name"`
	}
	if err := json.Unmarshal([]byte(message), &request); err != nil {
		return nil, errors.New("invalid JSON format")
	}

	if request.Command != "SET_NAME" || strings.TrimSpace(request.Name) == "" {
		return nil, errors.New("invalid or missing name")
	}

	player := &domain.Player{Name: request.Name, Conn: conn}
	return player, nil
}

// handlePlayerCommands обрабатывает команды игрока
func (s *Server) handlePlayerCommands(conn net.Conn, reader *bufio.Reader, player *domain.Player) error {
	for {
		conn.Write([]byte("Enter a JSON command:\n"))
		message, err := reader.ReadString('\n')
		if err != nil {
			return errors.New("failed to read command")
		}

		response, err := s.processMessage([]byte(strings.TrimSpace(message)), player)
		if err != nil {
			response = s.createErrorResponse(5000, err.Error())
		}

		conn.Write(response)
		conn.Write([]byte("\n"))
	}
}