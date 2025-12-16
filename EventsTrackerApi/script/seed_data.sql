-- =================================================================
-- Script de Siembra (Seeding) DETALLADO para la base de datos eventstracker_test
-- =================================================================
-- Ejecutar este script DESPUÉS de haber aplicado las migraciones y sobre una base con datos existentes.
-- Versión 2.1: Ajustado para no colisionar con datos preexistentes.

-- Deshabilitar temporalmente la verificación de claves foráneas para evitar errores de orden de inserción.
SET FOREIGN_KEY_CHECKS=0;

-- NOTA: Se han eliminado las sentencias TRUNCATE para preservar los datos existentes.
-- Los nuevos IDs se han ajustado para comenzar después de los IDs existentes en el DUMP proporcionado.
-- Users: El último ID es 1, se empieza desde 2.
-- Locations: El último ID es 9, se empieza desde 10.
-- Tags: El último ID es 30, se empieza desde 31.
-- Events: El último ID es 20, se empieza desde 21.

-- 1. Insertar Usuarios de Ejemplo (15 usuarios)
-- Se utiliza el PasswordHash proporcionado para todos. La pass para ese hash es 123456
-- IsHost = 1 para usuarios que pueden crear eventos.
-- IDs ajustados: 1 -> 2, 2 -> 3, ..., 15 -> 16
INSERT INTO `Users` (`ID`, `FirstName`, `LastName`, `Email`, `Dni`, `PasswordHash`, `IsHost`, `Estado`, `Fecha_Creacion`, `Fecha_Actualizacion`, `FlagUpdateData`, `TelefonoArea`, `TelefonoNumero`, `AvatarUrl`, `Bio`) VALUES
(2, 'Juan', 'Perez', 'juan.perez@example.com', '30123456', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 1, 1, NOW(), NOW(), 0, '266', '4123456', 'https://i.pravatar.cc/150?u=juan.perez', 'Organizador de eventos de tecnología y música en San Luis.'),
(3, 'Maria', 'Gomez', 'maria.gomez@example.com', '32789012', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 1, 1, NOW(), NOW(), 0, '266', '4987654', 'https://i.pravatar.cc/150?u=maria.gomez', 'Amante del arte, la cultura y la gastronomía local.'),
(4, 'Carlos', 'Rodriguez', 'carlos.rodriguez@example.com', '31555666', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '2657', '555111', 'https://i.pravatar.cc/150?u=carlos.rodriguez', 'Apasionado por el deporte y las actividades al aire libre.'),
(5, 'Ana', 'Martinez', 'ana.martinez@example.com', '34111222', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '266', '4333222', 'https://i.pravatar.cc/150?u=ana.martinez', 'Developer y asistente frecuente a meetups de tecnología.'),
(6, 'Luis', 'Fernandez', 'luis.fernandez@example.com', '29888777', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 1, 1, NOW(), NOW(), 0, '2657', '654321', 'https://i.pravatar.cc/150?u=luis.fernandez', 'Músico y productor. Siempre buscando nuevos talentos.'),
(7, 'Laura', 'Lopez', 'laura.lopez@example.com', '35999888', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '266', '4111222', 'https://i.pravatar.cc/150?u=laura.lopez', 'Foodie de corazón. No me pierdo un festival gastronómico.'),
(8, 'Sergio', 'Diaz', 'sergio.diaz@example.com', '33444555', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '266', '4888777', 'https://i.pravatar.cc/150?u=sergio.diaz', 'Corredor de maratones y entusiasta del trekking.'),
(9, 'Sofia', 'Sanchez', 'sofia.sanchez@example.com', '36123123', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '2657', '456789', 'https://i.pravatar.cc/150?u=sofia.sanchez', 'Estudiante de cine. Me encantan los ciclos de cine debate.'),
(10, 'Martin', 'Romero', 'martin.romero@example.com', '30111222', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 1, 1, NOW(), NOW(), 0, '266', '4555666', 'https://i.pravatar.cc/150?u=martin.romero', 'Organizador de ferias de emprendedores y artesanos.'),
(11, 'Valentina', 'Torres', 'valentina.torres@example.com', '37888999', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '266', '4777888', 'https://i.pravatar.cc/150?u=valentina.torres', 'Disfruto de los talleres y actividades para niños con mi familia.'),
(12, 'Jorge', 'Gutierrez', 'jorge.gutierrez@example.com', '28765432', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '266', '4123789', 'https://i.pravatar.cc/150?u=jorge.gutierrez', 'Aficionado a la historia y los recorridos culturales.'),
(13, 'Camila', 'Rojas', 'camila.rojas@example.com', '38123456', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '2657', '333444', 'https://i.pravatar.cc/150?u=camila.rojas', 'Me encanta bailar. Voy a todos los eventos de música en vivo.'),
(14, 'Matias', 'Sosa', 'matias.sosa@example.com', '32987654', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '266', '4445555', 'https://i.pravatar.cc/150?u=matias.sosa', 'Gamer y programador. Asisto a todas las competencias de e-sports.'),
(15, 'Lucia', 'Benitez', 'lucia.benitez@example.com', '35654321', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 1, 1, NOW(), NOW(), 0, '266', '4654321', 'https://i.pravatar.cc/150?u=lucia.benitez', 'Chef profesional y organizadora de eventos culinarios.'),
(16, 'Nicolas', 'Ramirez', 'nicolas.ramirez@example.com', '31234567', '$2a$10$v6z1iI0cCclVYFUhYmuvuO9B8luqC2whrwmbiemJeusnU5yCm98oK', 0, 1, NOW(), NOW(), 0, '2657', '123123', 'https://i.pravatar.cc/150?u=nicolas.ramirez', 'Lector empedernido y participante de clubes de lectura.');

-- 2. Insertar Ubicaciones (Locations) en San Luis, Argentina
-- IDs ajustados: 1 -> 10, 2 -> 11, ..., 7 -> 16
INSERT INTO `Location` (`Id`, `PlaceName`, `Latitude`, `Longitude`, `Address`) VALUES
(10, 'Centro de Convenciones de San Luis', CAST(-33.2805 AS DECIMAL(10, 8)), CAST(-66.3153 AS DECIMAL(10, 8)), 'Autopista 25 de Mayo Km 7.5, San Luis'),
(11, 'Parque de las Naciones', CAST(-33.3148 AS DECIMAL(10, 8)), CAST(-66.3401 AS DECIMAL(10, 8)), 'Av. del Fundador y Av. Lafinur, San Luis'),
(12, 'Anfiteatro Ave Fénix', CAST(-33.7019 AS DECIMAL(10, 8)), CAST(-65.4664 AS DECIMAL(10, 8)), 'Av. del Libertador Gral. San Martín, Juana Koslay'),
(13, 'Potrero de los Funes', CAST(-33.2217 AS DECIMAL(10, 8)), CAST(-66.2250 AS DECIMAL(10, 8)), 'Circuito Internacional Potrero de los Funes, Potrero de los Funes'),
(14, 'Plaza Pringles', CAST(-33.3025 AS DECIMAL(10, 8)), CAST(-66.3355 AS DECIMAL(10, 8)), 'Rivadavia 700, San Luis'),
(15, 'Espacio Cultural "La Vía"', CAST(-33.3070 AS DECIMAL(10, 8)), CAST(-66.3450 AS DECIMAL(10, 8)), 'Av. Illia 200, San Luis'),
(16, 'Comedor Universitario UNSL', CAST(-33.3000 AS DECIMAL(10, 8)), CAST(-66.3100 AS DECIMAL(10, 8)), 'Av. Ejército de los Andes 950, San Luis');

-- 3. Insertar Etiquetas (Tags)
-- IDs ajustados: 1 -> 31, 2 -> 32, ..., 15 -> 45
INSERT INTO `Tags` (`Id`, `Name`, `CreatedAt`, `UpdatedAt`) VALUES
(31, 'Música Seed', NOW(), NOW()),
(32, 'Tecnología Seed', NOW(), NOW()),
(33, 'Gastronomía Seed', NOW(), NOW()),
(34, 'Arte Seed', NOW(), NOW()),
(35, 'Deporte Seed', NOW(), NOW()),
(36, 'Cine Seed', NOW(), NOW()),
(37, 'Teatro Seed', NOW(), NOW()),
(38, 'Feria Seed', NOW(), NOW()),
(39, 'Congreso Seed', NOW(), NOW()),
(40, 'Infantil Seed', NOW(), NOW()),
(41, 'Aire Libre Seed', NOW(), NOW()),
(42, 'Educación Seed', NOW(), NOW()),
(43, 'E-Sports Seed', NOW(), NOW()),
(44, 'Literatura Seed', NOW(), NOW()),
(45, 'Cultura Seed', NOW(), NOW());

-- 4. Insertar Eventos (10 eventos)
-- Status 1: Activo, 0: Finalizado, 2: Cancelado
-- IDs ajustados: 1 -> 21, 2 -> 22, ..., 10 -> 30
-- CreatorID y LocationId ajustados a los nuevos IDs.
INSERT INTO `Events` (`ID`, `Name`, `Description`, `StartDateTime`, `EndDateTime`, `Capacity`, `CreatorID`, `Status`, `FlyerUrl`, `LocationId`, `Price`, `RatingsCount`, `RatingsSum`) VALUES
(21, 'San Luis Dev Conference 2026', 'La conferencia de desarrollo de software más importante de la región. Charlas sobre IA, Cloud y Desarrollo Web.', '2026-05-20 09:00:00', '2026-05-21 18:00:00', 500, 2, 1, 'https://picsum.photos/seed/devconf/800/400', 10, 15000.00, 0, 0),
(22, 'Festival Gastronómico "Sabores de Cuyo"', 'Un fin de semana para disfrutar de la mejor comida regional, con shows en vivo y clases de cocina.', '2026-04-15 12:00:00', '2026-04-17 23:00:00', 2000, 15, 1, 'https://picsum.photos/seed/sabores/800/400', 11, 2000.00, 0, 0),
(23, 'Concierto de Rock en Ave Fénix', 'Las mejores bandas de rock de la provincia se juntan en una noche única. ¡No te lo pierdas!', '2026-06-12 21:00:00', '2026-06-13 02:00:00', 5000, 6, 1, 'https://picsum.photos/seed/rock/800/400', 12, 8000.00, 0, 0),
(24, 'Maratón "San Luis Corre" 10K', 'Participa de la maratón anual de la ciudad. Categorías para todas las edades.', '2025-11-23 09:00:00', '2025-11-23 12:00:00', 1500, 3, 0, 'https://picsum.photos/seed/maraton/800/400', 11, 3500.00, 0, 0),
(25, 'Feria de Artesanos y Emprendedores', 'Apoyá el talento local. Productos únicos hechos a mano, diseño y mucho más.', '2026-07-05 15:00:00', '2026-07-05 21:00:00', 800, 10, 1, 'https://picsum.photos/seed/feria/800/400', 14, 0.00, 0, 0),
(26, 'Ciclo de Cine Clásico Francés', 'Proyección de 3 clásicos del cine francés con debate posterior. Entrada libre y gratuita.', '2026-08-10 19:00:00', '2026-08-10 22:00:00', 100, 3, 1, 'https://picsum.photos/seed/cine/800/400', 15, 0.00, 0, 0),
(27, 'Taller de Programación para Niños', 'Una introducción divertida al mundo del código para chicos de 8 a 12 años.', '2026-09-19 10:00:00', '2026-09-19 12:00:00', 30, 2, 1, 'https://picsum.photos/seed/kids/800/400', 16, 1000.00, 0, 0),
(28, 'Torneo de League of Legends "Copa Sierras"', 'Competencia de e-sports con los mejores equipos de la región. ¡Inscribí a tu team!', '2026-10-03 10:00:00', '2026-10-04 20:00:00', 200, 2, 1, 'https://picsum.photos/seed/lol/800/400', 10, 500.00, 0, 0),
(29, 'Encuentro de Clubes de Lectura', 'Un espacio para compartir y debatir sobre "Cien años de soledad". Coordina: Nicolás Ramirez.', '2026-07-25 18:00:00', '2026-07-25 20:00:00', 50, 16, 1, 'https://picsum.photos/seed/lectura/800/400', 15, 0.00, 0, 0),
(30, 'Trekking al Salto de la Moneda', 'Salida grupal de trekking de dificultad media. Un día para conectar con la naturaleza.', '2026-05-01 08:00:00', '2026-05-01 17:00:00', 25, 6, 2, 'https://picsum.photos/seed/trekking/800/400', 13, 4000.00, 0, 0);

-- 5. Asociar Etiquetas a Eventos (tabla intermedia EventTags)
-- EventId y TagId ajustados a los nuevos IDs.
INSERT INTO `EventTags` (`EventId`, `TagId`, `LinkedAt`, `Order`) VALUES
-- San Luis Dev Conference
(21, 32, NOW(), 1), (21, 39, NOW(), 2), (21, 42, NOW(), 3),
-- Festival Gastronómico
(22, 33, NOW(), 1), (22, 31, NOW(), 2), (22, 38, NOW(), 3), (22, 41, NOW(), 4),
-- Concierto de Rock
(23, 31, NOW(), 1),
-- Maratón
(24, 35, NOW(), 1), (24, 41, NOW(), 2),
-- Feria de Artesanos
(25, 38, NOW(), 1), (25, 34, NOW(), 2), (25, 45, NOW(), 3),
-- Ciclo de Cine
(26, 36, NOW(), 1), (26, 45, NOW(), 2),
-- Taller para Niños
(27, 40, NOW(), 1), (27, 42, NOW(), 2), (27, 32, NOW(), 3),
-- Torneo de LoL
(28, 43, NOW(), 1), (28, 32, NOW(), 2),
-- Club de Lectura
(29, 44, NOW(), 1), (29, 45, NOW(), 2),
-- Trekking (Cancelado)
(30, 35, NOW(), 1), (30, 41, NOW(), 2);

-- 6. Insertar Invitaciones a Eventos
-- ResponseStatus: 'Pending', 'Accepted', 'Declined'
-- IDs ajustados para EventId, UserId, CreatorId.
INSERT INTO `EventInvitations` (`Id`, `EventId`, `UserId`, `CreatorId`, `ResponseStatus`, `SentDate`, `ResponseDate`) VALUES
-- Invitaciones para la Dev Conf
(1, 21, 5, 2, 'Accepted', '2026-03-01 10:00:00', '2026-03-02 11:00:00'), -- Juan (creador) invita a Ana, y acepta
(2, 21, 14, 2, 'Pending', '2026-03-01 10:01:00', NULL), -- Juan invita a Matias, pendiente
(3, 21, 7, 2, 'Declined', '2026-03-01 10:02:00', '2026-03-03 18:30:00'), -- Juan invita a Laura, y rechaza
-- Invitaciones para el Festival Gastronómico
(4, 22, 7, 15, 'Accepted', '2026-03-20 15:00:00', '2026-03-20 15:05:00'), -- Lucia (creadora) invita a Laura, y acepta
(5, 22, 2, 15, 'Accepted', '2026-03-20 15:01:00', '2026-03-21 09:00:00'), -- Lucia invita a Juan, y acepta
-- Invitaciones para el Concierto de Rock
(6, 23, 13, 6, 'Pending', '2026-05-15 12:00:00', NULL); -- Luis (creador) invita a Camila, pendiente

-- 7. Insertar Posts (Comentarios) en Eventos
-- IDs ajustados para EventID y UserID.
INSERT INTO `EventPosts` (`ID`, `EventID`, `UserID`, `Text`, `CreationDate`) VALUES
-- Comentarios en la Dev Conf
(1, 21, 5, '¡Qué buena agenda! No me pierdo la charla de Cloud Native.', '2026-03-05 14:00:00'),
(2, 21, 14, '¿Habrá lugar para estacionar cerca del centro de convenciones?', '2026-03-06 09:30:00'),
(3, 21, 2, '¡Hola Matías! Sí, el Centro de Convenciones cuenta con estacionamiento propio y gratuito. ¡Te esperamos!', '2026-03-06 11:00:00'),
-- Comentarios en el Festival Gastronómico
(4, 22, 7, '¡Por fin un evento así! ¿Estarán los food trucks de siempre? ¡Muero por las empanadas de "La Chacha"!', '2026-03-22 19:00:00'),
(5, 22, 15, '¡Hola Laura! Confirmado que "La Chacha" estará con nosotros. Además, ¡tendremos 5 puestos nuevos este año! No te los pierdas.', '2026-03-23 10:00:00'),
-- Comentarios en el evento finalizado (Maratón)
(6, 24, 8, '¡Excelente organización! El recorrido fue duro pero increíble. ¿Para cuándo la próxima?', '2025-11-23 13:00:00'),
(7, 24, 3, 'Gracias a todos por participar. ¡Ya estamos trabajando en la edición 2026! Estén atentos a las novedades.', '2025-11-24 09:00:00');

-- 8. Insertar Calificaciones (Ratings) de Eventos
-- Solo para eventos finalizados. Score es de 1 a 10.
-- IDs ajustados para EventId y UserId.
INSERT INTO `EventRatings` (`EventId`, `UserId`, `Score`, `CreatedAt`, `UpdatedAt`) VALUES
(24, 8, 9, '2025-11-23 14:00:00', '2025-11-23 14:00:00'), -- Sergio califica la maratón
(24, 4, 8, '2025-11-23 15:30:00', '2025-11-23 15:30:00'); -- Carlos califica la maratón

-- Actualizar los contadores de ratings en la tabla de Eventos
UPDATE `Events` SET `RatingsCount` = 2, `RatingsSum` = (9 + 8) WHERE `ID` = 24;

-- Reactivar la verificación de claves foráneas
SET FOREIGN_KEY_CHECKS=1;

-- Fin del script.