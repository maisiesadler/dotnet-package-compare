# Dotnet Package Compare

Shows dotnet package differences on a PR.

## Example configuration

```
name: Compare Packages
on:
  pull_request

jobs:
  compare_packages:
    name: Compare packages to main
    runs-on: ubuntu-latest
    permissions:
      pull-requests: write
    steps:
      - uses: actions/checkout@v1
      - name: List packges before and after 
        id: list_packages
        shell: bash
        run: |
          git checkout main
          dotnet restore
          dotnet list package --include-transitive > packages-before.txt

          git checkout -
          dotnet restore
          dotnet list package --include-transitive > packages-after.txt

      - name: Run package comparison
        id: run_package_comparison
        uses: maisiesadler/dotnet-package-compare@v0.0.1-pre03
        env:
          BEFORE_FILE_LOCATION: './packages-before.txt'
          AFTER_FILE_LOCATION: './packages-after.txt'

      - name: Comment PR
        uses: thollander/actions-comment-pull-request@v2
        if: ${{ env.DIFF_OUTPUT }}
        with:
          message: |
            ${{ env.DIFF_OUTPUT }}
```

Use a comment_tag to update one comment instead of creating a new comment per commit:

```
      - name: Comment PR
        uses: thollander/actions-comment-pull-request@v2
        if: ${{ env.DIFF_OUTPUT }}
        with:
          comment_tag: package_diffs
          message: |
            ${{ env.DIFF_OUTPUT }}
```

## Example Output

```
## Changed Packages
### PackageLister
### PackageLister.Test
#### net7.0 (4 differences)
##### Added (4)
- AutoFixture [4.18.0 (direct)]
- Fare [2.1.1 (transient)]
- System.ComponentModel [4.3.0 (transient)]
- System.ComponentModel.Annotations [4.3.0 (transient)]

### PackageLister.Console
```
