-- Minipower trace index (ADR-033 §5.4) — projection, không chứa body FR/SRS.
-- Copy lúc init → memory/trace.sql ; DB = memory/trace.db (gitignore).
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS artifact (
  id            TEXT PRIMARY KEY,
  type          TEXT NOT NULL,
  title         TEXT NOT NULL DEFAULT '',
  status        TEXT NOT NULL DEFAULT 'draft',
  provider_face TEXT,
  provider      TEXT,
  provider_uri  TEXT,
  rev           TEXT,
  updated_at    TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS artifact_type ON artifact (type);
CREATE INDEX IF NOT EXISTS artifact_provider ON artifact (provider_face, provider);

CREATE TABLE IF NOT EXISTS link (
  from_id  TEXT NOT NULL REFERENCES artifact(id) ON DELETE CASCADE,
  to_id    TEXT NOT NULL REFERENCES artifact(id) ON DELETE CASCADE,
  rel      TEXT NOT NULL,
  PRIMARY KEY (from_id, to_id, rel)
);

CREATE INDEX IF NOT EXISTS link_to ON link (to_id);

CREATE TABLE IF NOT EXISTS face_binding (
  face       TEXT PRIMARY KEY,
  provider   TEXT NOT NULL,
  mcp_server TEXT
);

CREATE TABLE IF NOT EXISTS migration (
  id            INTEGER PRIMARY KEY AUTOINCREMENT,
  face          TEXT NOT NULL,
  from_provider TEXT NOT NULL,
  to_provider   TEXT NOT NULL,
  status        TEXT NOT NULL,
  started_at    TEXT,
  finished_at   TEXT,
  note          TEXT
);

CREATE INDEX IF NOT EXISTS migration_face_status ON migration (face, status);
