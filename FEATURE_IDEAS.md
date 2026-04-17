# Feature Ideas for Phoronix Result Viewer

This is a backlog of possible features for future versions of the app.

## Import and Data Handling

- Import multiple result JSON files in one action and merge them into one comparison session.
- Add drag-and-drop support for result files directly onto the main window.
- Support loading result files from URL (openbenchmarking.org links) in addition to local files.
- Validate imported JSON and show friendly error details when parsing fails.
- Keep an import history so users can quickly reload recent result sets.
- Let users map custom display names for systems (for cleaner chart labels).
- Detect duplicate systems/tests during import and offer deduplication options.
- Add optional support for CSV export/import for users who preprocess data outside the app.

## Comparison and Analysis

- Add side-by-side baseline comparison mode (System A vs System B, not only one base vs all).
- Add median and percentile summaries next to geometric mean.
- Add variance/stability indicators when tests contain multiple runs.
- Show confidence or reliability score for each suite based on test count and spread.
- Let users set custom weights per test suite for overall score calculation.
- Add profile presets (CPU-focused, GPU-focused, efficiency-focused, workstation-focused).
- Add outlier detection and optional outlier exclusion toggle.
- Add normalized score options (z-score, min-max) besides current relative scaling.
- Add cross-suite trend insights such as "best efficiency gain" and "largest regression".
- Add a quick "wins/losses" summary table per system.

## Filtering and Discovery

- Add search box for suite/test names and identifiers.
- Add filters by category (CPU, memory, storage, graphics, power).
- Add filters by unit/scale and performance class (higher-is-better/lower-is-better).
- Add tags or bookmarks for interesting suites/tests.
- Add advanced filtering rules (AND/OR expressions for identifiers and metadata).
- Save and load filter configurations as named views.

## Visualization and UI

- Add chart type switching (bar, radar, line for historical runs, heatmap for suite overview).
- Add sorting controls for charts (alphabetical, best-to-worst, custom order).
- Add tooltips with raw values, normalized values, and percent delta from base.
- Add inline annotations/notes on charts for key findings.
- Add compact mode optimized for small screens.
- Add keyboard shortcuts for import, filtering, switching base system, and export.
- Add customizable color palettes with colorblind-safe defaults.
- Add toggle to display value labels directly on bars.
- Add split-pane resizing and persistent window/layout state.

## Reporting and Export

- Export chart images (PNG/SVG) per suite and as batch.
- Export full report as Markdown/HTML/PDF with charts and summary tables.
- Add "share package" export that includes data + report + app metadata.
- Add one-click copy of summary stats for posting to forums or issue trackers.
- Add report templates (short executive summary vs full technical deep-dive).

## Session and Project Workflow

- Add project/session files so users can save and reopen full comparison state.
- Auto-save unsaved sessions and recover after crash.
- Add comparison snapshots so users can capture before/after states.
- Add notes panel for recording BIOS/kernel/driver settings per session.
- Add run metadata editor for environment details (kernel, governor, RAM config, etc.).

## Online and Integration

- Add optional sync with OpenBenchmarking account data.
- Fetch latest related public runs for a selected test suite and compare locally.
- Add plugin hooks for custom metrics and custom post-processing logic.
- Add command-line mode for headless report generation from JSON inputs.
- Add simple local API endpoint for automation from scripts.

## Quality, Reliability, and Performance

- Add structured logging with log viewer and export for troubleshooting.
- Add clear user-facing error states instead of silent exception ignores.
- Add background loading with progress indicators for large datasets.
- Add caching invalidation controls for test suite metadata.
- Add benchmark data integrity checks (missing test pairs, inconsistent units).
- Add telemetry-free diagnostics bundle generator for bug reports.
- Add performance optimizations for very large result sets (virtualization, lazy compute).

## Developer Experience and Testing

- Add automated unit tests for parsing, normalization, and suite aggregation logic.
- Add snapshot tests for report generation output.
- Add integration tests with sample Phoronix JSON fixtures.
- Add CI pipeline to run tests and produce packaged desktop artifacts.
- Add docs page describing data model and score formulas.

## Nice-To-Have Stretch Ideas

- Add "recommendation" view that suggests best system by user goal.
- Add hardware cost input and performance-per-dollar analysis.
- Add power price input and estimated annual energy-cost impact.
- Add regression alerts when comparing against a saved baseline project.
- Add lightweight "presentation mode" for sharing results live.
