// Shared configuration for the Playwright Java test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 mvn test

package emergencycaresystem.support;

public final class Config {
    public static final String BASE_URL =
            System.getenv("BASE_URL") != null ? System.getenv("BASE_URL") : "http://localhost:5173";

    private Config() {}
}
