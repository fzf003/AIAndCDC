## ADDED Requirements

### Requirement: Load .env file on startup
The system SHALL automatically load the `.env` file from the project directory into process environment variables before the Host builder initializes.

#### Scenario: .env file exists
- **WHEN** the application starts and `CdcRedisConsumer/.env` exists
- **THEN** all `KEY=VALUE` pairs (excluding comments and empty lines) are loaded into `Environment.GetEnvironmentVariable`

#### Scenario: .env file missing
- **WHEN** the application starts and `.env` does not exist
- **THEN** startup continues without error, relying on system environment variables or `appsettings.json` defaults

#### Scenario: .env contains comments and blank lines
- **WHEN** `.env` contains lines starting with `#` or empty lines
- **THEN** those lines are ignored and valid key-value pairs are still loaded

### Requirement: Resolve ${VAR} placeholders in configuration values
The system SHALL support resolving `${VAR}` placeholders in configuration values read from `appsettings.json` by substituting them with corresponding environment variables.

#### Scenario: Placeholder resolves successfully
- **WHEN** `appsettings.json` contains `"Redis:Url": "${REDIS_HOST}:${REDIS_PORT}"` and both environment variables are set
- **THEN** the resolved value is `"127.0.0.1:6379"` (or whatever the variables contain)

#### Scenario: Placeholder for StreamKey
- **WHEN** `appsettings.json` contains `"Redis:StreamKey": "product.${DATABASE_NAME}.dbo.Product"` and `DATABASE_NAME` is set
- **THEN** the resolved value substitutes `${DATABASE_NAME}` with the actual database name

#### Scenario: Unresolved placeholder remains literal
- **WHEN** a placeholder references an environment variable that does not exist in `.env` or system environment
- **THEN** the placeholder string `${VAR}` remains in the configuration value and downstream connection logic fails with a clear error

### Requirement: Configuration precedence
The system SHALL resolve configuration values with the following precedence: system environment variables > `.env` file > `appsettings.json` defaults.

#### Scenario: System env overrides .env
- **WHEN** system environment has `REDIS_HOST=192.168.0.103` and `.env` has `REDIS_HOST=127.0.0.1`
- **THEN** the resolved value uses `192.168.0.103`

#### Scenario: .env overrides appsettings default
- **WHEN** `.env` has `REDIS_HOST=192.168.0.103` and `appsettings.json` has `"${REDIS_HOST}"`
- **THEN** the resolved value uses `192.168.0.103`
