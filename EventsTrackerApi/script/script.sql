-- 1) Habilitar el scheduler (una vez por servidor)
SET GLOBAL event_scheduler = ON;
-- Para dejarlo permanente: en my.cnf -> [mysqld] event_scheduler=ON

-- 2) (Opcional pero recomendado) índice para acelerar el barrido
CREATE INDEX IX_Events_EndDateTime_Status ON events (EndDateTime, Status);

-- 3) Evento recurrente
DROP EVENT IF EXISTS ev_mark_finished_events;
CREATE EVENT ev_mark_finished_events
    ON SCHEDULE EVERY 1 MINUTE
    ON COMPLETION PRESERVE
    DO
      UPDATE events
      SET Status = 0
      WHERE EndDateTime < NOW(6)
        AND Status <> 0;