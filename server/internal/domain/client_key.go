package domain

type ClientKey struct {
	Username string
	Password string
}

func NewClientKey(username, password string) ClientKey {
	return ClientKey{
		Username: username,
		Password: password,
	}
}
