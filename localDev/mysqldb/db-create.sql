CREATE DATABASE `sessions` /*!40100 DEFAULT CHARACTER SET latin1 */;

USE `sessions`;

CREATE TABLE `profiles` (
  `id` char(36) NOT NULL,
  `forename` varchar(255) NOT NULL,
  `surname` varchar(255) NOT NULL,
  `rating` int(11) NOT NULL,
  `imageUrl` varchar(255) DEFAULT NULL,
  `bio` text,
  `isAdmin` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE `handles` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `type` varchar(50) NOT NULL,
  `identifier` varchar(100) NOT NULL,
  `profileId` char(36) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `profile_handle_fk` (`profileId`),
  CONSTRAINT `profile_handle_fk` FOREIGN KEY (`profileId`) REFERENCES `profiles` (`id`) ON DELETE CASCADE ON UPDATE NO ACTION
) ENGINE=InnoDB AUTO_INCREMENT=193 DEFAULT CHARSET=latin1;

CREATE TABLE `sessions` (
  `id` char(36) NOT NULL,
  `title` varchar(255) DEFAULT NULL,
  `status` varchar(31) DEFAULT 'unassigned',
  `speakerId` char(36) NOT NULL,
  `adminId` char(36) DEFAULT NULL,
  `dateAdded` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `date` datetime DEFAULT NULL,
  `description` text,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE `events` (
  `id` char(36) NOT NULL,
  `date` datetime NOT NULL,
  `name` varchar(256) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;


CREATE USER 'sessions'@'%' IDENTIFIED BY 'apassword';
GRANT ALL PRIVILEGES ON sessions. * TO 'sessions'@'%';
FLUSH PRIVILEGES
