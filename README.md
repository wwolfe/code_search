# code_search
Tool to search code

Tool searches for a string and displays the results for each file the string is present in.  Allows the user to jump directly to the file and line the string is located at in the selected file.

Warning: does a find from your current directory with no depth limit. So do not run in root dir

Add command to search path

Usage: cs [-i|-n expression_for_file_search ] reg_exp_pattern
   -i Ignores the case
     -n expression  must escape * with \

example: cs -n *txt pattern


## Ideas

- Make go util
- Make work in shell but not loose history of shell so you get your screen contents back
- List exclude directories
- List file extentions searched check uncheck add to list remove from list
- When you modify a file mark as modified
- Re-load from cache or current list
- Scenario you want to make the same edit in multiple files.  you want to know which ones you edited sometimes you just want to mark as done sometimes you want to just refresh the list so it disappears.
- Cache file list so it does not have to run find again.
- Exclude using .gitignore
