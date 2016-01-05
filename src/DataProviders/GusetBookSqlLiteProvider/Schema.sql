﻿CREATE TABLE IF NOT EXISTS [User] (
   UserId   INTEGER  PRIMARY KEY AUTOINCREMENT,
   [Login]	NVARCHAR(64) NOT NULL,
   Display  NVARCHAR(256),
   Created  DATETIME NOT NULL   ,
   CONSTRAINT 'idx_login_unique' UNIQUE([Login])
);

CREATE TABLE IF NOT EXISTS [Message] (
	MessageId INTEGER PRIMARY KEY AUTOINCREMENT,
	[Text]   NVARCHAR(4096) NOT NULL,
	Created  DATETIME NOT NULL,
	UserId   INT NOT NULL,
	CONSTRAINT 'fk_UserId'
		FOREIGN KEY(UserId) REFERENCES [User](UserId) ON DELETE CASCADE
);
