// Regex assertion used for the `assert.match(text, /pattern/i)` checks:
// passes when the pattern is found anywhere in the text.

package emergencycaresystem.support;

import static org.junit.jupiter.api.Assertions.fail;

import java.util.regex.Pattern;

public final class Matchers {
    private Matchers() {}

    public static void assertMatches(String actual, String regex, boolean ignoreCase) {
        Pattern pattern = Pattern.compile(regex, ignoreCase ? Pattern.CASE_INSENSITIVE : 0);
        if (!pattern.matcher(actual).find()) {
            fail("Expected \"" + actual + "\" to match /" + regex + "/" + (ignoreCase ? "i" : ""));
        }
    }
}
