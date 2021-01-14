#!/usr/bin/perl -w

####################################################################################
# Created by Ranjeet Shetye
# Maintaned by Wendell Wolfe
# Date: 01/21
####################################################################################

$Usage = "Usage: cs [-i|-n expression_for_file_search ] reg_exp_pattern\n\t -i Ignores the case\n\t -n expression  must escape \* with \\ \n\t  example: cs -n *txt pattern";
			
#$Usage = "Usage: cs [-s|-i|-c|-l] reg_exp_pattern";

if ($#ARGV == -1) {
  print "$Usage\n";
  exit 1;
}

# BEGIN set options
# ======================
# section dealing with command line options
$caseless = 0; # FALSE defines whether the pattern search should be case sensitive or not
$perl_subroutine = 0; # FALSE defines whether we are searching for a perl sub routine or not
$multi_window = 0; # TRUE defines whether cscope should use multiple windows when drilling down
$create_index_file = 0; # FALSE Creates a file index instead of using find
#$file_index = "/admin/scripts/gridmill/cs_file_index.dat"; # file list to use instead of find
#$file_index = "/home/ww017313/cs_file_index.dat"; # file list to use instead of find
$use_index_filelist = 0; # FALSE use the index file list
$max_find_depth = 0; # only used if greater than 0
$rfind_options = "";

# sections dealing with nitty gritties of displaying output
$show_n_lines=30; # maximum num of lines shown per window, more lines need one to go forward or backward
$length_linecontent = 100; # defines the max number of characters to be displayed per line
$max_filename_length=40; # the maximum limit on the filename that will be displayed
$current_max_filename_length=15; # defines the longest filename encountered till now
$strip_leading_white_space = 0; # whether each line should be stripped of leading white space or not
$find_name = ""; # 
$cmd_search = 0; # FALSE used to use specific files to search

# END set options
# ======================

# the pattern "[^HIFN_IO]_IO" will find all patterns "_IO" except those containing the pattern "HIFN_IO"

$pattern = "";
# $rgrep = "rgrep ";
##-type d \\( -path storage -o - tmp -o -path public \\) -prune -false -o \\
#
$rgrep_options = "";
$rgrepA = 'find . \\
\( \\
-type d \\
-name node_modules -prune -false \\
-o -name s3_images -prune -false \\
-o -name storage -prune -false \\
-o -name tmp -prune -false \\
-o -name public -prune -false \\
-o \\
-type f \\
-name \'*.ksh\' \\
-o -name \'*.iml\' \\
-o -name \'*.scala\' \\
-o -name \'*.groovy\' \\
-o -name \'*.gradle\' \\
-o -name \'*.erb\' \\
-o -name \'*.md\' \\
-o -name \'*.rb\' \\
-o -name \'*.yml\' \\
-o -name \'*.haml\' \\
-o -name \'*.js\' \\
-o -name \'*.json\' \\
-o -name \'*.sh\' \\
-o -name \'*.err\' \\
-o -name \'*.out\' \\
-o -name \'*.txt\' \\
-o -name \'*.ccl\' \\
-o -name \'*.CCL\' \\
-o -name \'*.csv\' \\
-o -name \'*.map\' \\
-o -name \'*.prg\' \\
-o -name \'*.PRG\' \\
-o -name \'*.inc\' \\
-o -name \'*.sql\' \\
-o -name \'*.def\' \\
-o -name \'*.ora\' \\
-o -name \'*.in\' \\
-o -name \'*.xslt\' \\
-o -name \'*.html\' \\
-o -name \'*.java\' \\
-o -name \'*.sub\' \\
-o -name \'*.ekm\' \\
-o -name \'*.ard\' \\
-o -name \'*.scss\' \\
-o -name \'*.go\' \\
-o -name \'*.css\' \\
-o -name \'*.vue\' \\
-o -name \'Jenkinsfile\' \\
-o -name \'*.jbuilder\' \\
-o -name \'*.haml\' \\
\) ';
#-o -name \'*.log\' \\
#-o -name \'*.c\' \\
#-o -name \'*.s\' \\
#-o -name \'*.xml\' \\
#-o -name \'*akefile*\' \\
#-o -name \'files.*\' \\
# -o -name \'*.pl\' \\
# -o -name \'*.cgi\' \\
# -o -name \'*.html\' \\
# -o -name \'install.FreeBSD\' \\
# -o -name \'*.sh\' \\

$rgrepB = '\'';
$rgrepC = '\' {} \\;';

$grep = "grep -n ";

$ENV{CSCOPE}="1";

use Term::ANSIColor;

while ($#ARGV > -1) {

  $cmd_option = shift @ARGV;

#  print "$cmd_option\n";
#  print "$#ARGV\n";

  # always just use the last arg as the search pattern, else it gets parsed as options
  if ($#ARGV == -1) {
    if ($pattern ne "") {
      $pattern .= " ";
    }
    $pattern .= $cmd_option;
  }
  elsif ($cmd_option =~ /-i/) { # insensitive (case search)
    $caseless = 1; # TRUE
  }
  elsif ($cmd_option =~ /-ps/) { # perl subroutine
    $perl_subroutine = 1; # TRUE
  }
  elsif ($cmd_option =~ /-sw/) { # single window
    $multi_window = 0; # FALSE
  }
  elsif ($cmd_option =~ /-d/) { # perl subroutine
    $max_find_depth = shift @ARGV;
  }
  elsif ($cmd_option =~ /-c/) { # Create index
    $create_index_file = 1; # TRUE
  }
  elsif ($cmd_option =~ /-l/) { # Create index
  	$use_index_filelist = 1; # TRUE
  }
  elsif ($cmd_option =~ /-n/) { # Create index
  	$cmd_search = 1; # TRUE
    $cmd_option = shift @ARGV;
  	$find_name = $cmd_option;
  }
  else {
    #print "$cmd_option\n";
    if ($pattern ne "") {
      $pattern .= " ";
    }
    $pattern .= $cmd_option;
  }
}

if ($cmd_search == 1) {
	$rgrepA = "find . -name \"$find_name\" -type f -exec grep -l ";
}
#print "$rgrepA";
#exit;

if ($caseless == 1) {
  $rgrep_options = " -i ";
  $grep .= "-i ";
}

if ($max_find_depth > 0) {
  $depth = sprintf "%d", $max_find_depth;
  $rfind_options = "-maxdepth $depth ";
}

if ($perl_subroutine) {
  $pattern = "sub.*" . "$pattern";
}


$rgrep = "$rgrepA" . "$rfind_options" . "-exec grep -l " . "$rgrep_options" . "$rgrepB" . "$pattern" . "$rgrepC";
#print "$rgrep\n";
#exit;

if ($create_index_file == 1 ) {
	print (" RUN as root from the root directory ");
	system ("$rgrep > $file_index");
	exit;
}

#exit;
#$ret_stat = system ("which xterm >> /dev/null");
# $ret_stat = system ("which GARBAGE >> /dev/null");
# $ret_stat %= 255;
# print (" ret_stat is : $ret_stat \n");
#if ( ($ret_stat == 0) && ($multi_window == 1) ) { # xterm exists
#  $XtermExec = "xterm -sb -sl 4000 -geometry 131x66+30+1 -fn 8x13 -e $ENV{SHELL} -exec ";
#  $XtermExec = "xterm -e $ENV{SHELL} -exec ";
#  $multi_window = 1; # TRUE
#}
#else { # xterm does not exist or user has explicitly asked for single window
#  # $XtermExec = "$ENV{SHELL} -exec ";
#  $multi_window = 0; # FALSE
#}

sub SystemExec {
  local ($outstr) = @_;

#   print ("outstr is : $outstr \n");
  system ("$outstr");

#  if ($multi_window == 0) {
#    system ("$outstr");
#  }
#  else {
#    system ("$XtermExec \'$outstr\'");
#  }
}

chomp($pwd=`pwd`);
#$main_window_title="D$ENV{SHLVL} Looking for $pattern under " . $pwd ;
$main_window_title=" Looking for $pattern under " . $pwd ;
# print "$main_window_title\n";
#system(" echo -n \"\033]0;$main_window_title\07\" ");
system(" echo \"\033]0;$main_window_title\07\" ");

# if ( ($ARGV[0] eq "-s") and ($#ARGV == 1) ) {
#   $pattern = "sub.*$ARGV[1]";
# }
# elsif ( ($ARGV[0] eq "-i") and ($#ARGV == 1) ) {
#   $pattern = "$ARGV[1]";
#   $rgrep .= "-i ";
#   $grep .= "-i ";
# }

# @files=`$rgrep \'$pattern\'`;
#@files=`$rgrep`;

if ($use_index_filelist == 1 ) {
	@files=`cat $file_index`;
}
else {
#  print "$rgrep";
# exit;
	@files=`$rgrep`;
}

$num_of_files=$#files;

#print "$num_of_files";

$count=1;
$files="";

if ($#files != -1) {
  while (@files) {
    $files .= shift @files;
    $files .= " ";
  }
  $files =~ s/\n//g;
#print "$grep \'$pattern\' $files\n";
  @lines=`$grep \'$pattern\' $files`;
#print @lines;
#exit;
#print "\n$files\n";
#exit;
  if ($#lines != -1) {
    foreach $line (@lines) {
      # only one file, then grep returns a diff o/p
      if ($num_of_files == 0) {
        # grep returns: "104:  clouddev: CareAware_NonProd_US"
        @fields = split (":", $line, 2);
        $filename = $files;
      }
      else {
        # grep returns: "./revcycle/stack/clouddev.yml.erb:48:- clouddev"
        @fields = split (":", $line, 3);
        $filename = shift @fields;
      }
      $filename =~ s/^\.\///;
      $current_filename_length = length($filename);
      if ($current_max_filename_length < $max_filename_length) {
        if ($current_filename_length > $current_max_filename_length) {
          if ($current_filename_length < $max_filename_length) {
            $current_max_filename_length = $current_filename_length
          }
          else {
            $current_max_filename_length = $max_filename_length;
          };
        };
      };
      $linenum = shift @fields;
      $linecontent = shift @fields;
      $linecontent =~ s/\n//g;
      if (length($linecontent) > $length_linecontent) {
        $linecontent = substr ($linecontent, 0, $length_linecontent);
      }
      $linecontent .= "\n";
      if ($strip_leading_white_space == 1) {
        $linecontent =~ s/^\s*/ /g;
      };
      # print "$count ${file}:${linenum} ${linecontent}";
      $entry = join ':', $filename, $linenum, $linecontent;
      $ary[$count] = $entry;
      $count++;
    }
  }
}

$total_entries=$count-1;
$count=1;
# print "@ary";
$temp="";

$input="a";
$count=1;
while (1) {
  system("clear");

  $start_line=$count;
  $end_line = $count + ($show_n_lines-1);
  if ($end_line > $total_entries) {
    $end_line = $total_entries;
  }

  print color('bold blue');
  print "Searching for pattern : $pattern \t\t\t\t $total_entries Entries found\n";
  print "  enter $start_line-$end_line, 'f' to go forward, 'b' to go back, or 'q' to quit\n\n";
  print color('reset');
  $i = $start_line;
  # while ( ($i <= $total_entries) && ($i <= $count+($show_n_lines-1)) )
  while ($i <= $end_line) {
    $entry = $ary[$i];
    ($filename, $linenum, $linecontent) = split (/:/, $entry, 3);
    #print STDOUT sprintf "%3d %-40s : %4d : %-s", $i, $filename, $linenum, $linecontent;
    #print STDOUT sprintf "%3d %-${current_max_filename_length}s : %4d :\n", $i, $filename, $linenum;
    #print STDOUT sprintf "%3d %-${current_max_filename_length}s : %4d : %-s", $i, $filename, $linenum, $linecontent;
    print color('yellow');
    $i_display = sprintf "%4d", $i;
    print "$i_display";
    print color('reset');
    print color('magenta');
    $linenum_display = sprintf " %4d - ", $linenum;
    print $linenum_display;
    print color('reset');
    print color('green');
    $filename_display = sprintf "%-${current_max_filename_length}s ", $filename;
    print "$filename_display";
    print color('reset');
    $linecontent_display = sprintf "%s", $linecontent;
    print color('cyan');
    print $linecontent_display;
    print color('reset');
#    print STDOUT sprintf "%3d %-${current_max_filename_length}s : %4d : %s", $i, $filename, $linenum, $linecontent;

    $i++;
  }
  if ($end_line < $total_entries) {
    print color('bold blue');
    print "\nMore ...";
    print color('reset');
  }
  print "\n";

  $input="a";
  $input = <STDIN>;
  chop $input;

  if ($input eq "") {
    $input = "f"
  }

  if ($input =~ /^\!/) {
    $input =~ s/^\!//;
    if ($input eq "sh") {
      SystemExec("$ENV{SHELL}");
    }
    elsif ($input =~ /^cs /) {
      SystemExec("$input");
    }
    else {
      SystemExec("$input");
      print "Please press enter to continue ...";
      $temp = <STDIN>;
    }
  }
  elsif ( ($input ge "a") && ($input le "z") ) {
    if ($input eq "q") {
      exit;
    }
    elsif ($input eq "f") {
      # print "$input\n";
      # print "count: $count, \ntotal_entries: $total_entries, \nshow_n_lines: $show_n_lines, ";
      # (<STDIN>);
      if ($total_entries-($count+($show_n_lines-1)) > 0) {
        $count += $show_n_lines;
      }
      # elsif ($count > 9) {
        # $count = $total_entries-9;
      # }
      else {
        $count = 1;
      }
    }
    elsif ($input eq "b") {
      if ($count > ($show_n_lines-1)) {
          $count -= $show_n_lines;
      }
      else {
        # $count = 1;
        $count = $total_entries - ($total_entries % $show_n_lines) + 1;
      }
    }
  }
  elsif ( ($input eq "") && ($total_entries == 1) ) {
    $input = 1;
    ($filename, $linenum, $linecontent) = split(":", $ary[$input], 3);
    # print "$linenum, $filename";
    #$Win_title="D$ENV{SHLVL} Option $input : found $pattern in $filename on
    $Win_title=" Option $input : found $pattern in $filename on line $linenum";
    SystemExec ("echo -n \"\033]0;$Win_title\07\"; vi -c $linenum $filename; echo -n \"\033]0;$main_window_title\07\" ");
    #SystemExec ("echo \"\033]0;$Win_title\07\"; gvim -c $linenum $filename; echo \"\033]0;$main_window_title\07\" ");
  }
  elsif ($input =~ /\d\d*/) {
    if ( ($input >= 1) && ($input <= $total_entries) ) {
      ($filename, $linenum, $linecontent) = split(":", $ary[$input], 3);
      # print "$linenum, $filename";
      #$Win_title="D$ENV{SHLVL} Option $input : found $pattern in $filename
      $Win_title=" Option $input : found $pattern in $filename on line $linenum";
      SystemExec ("echo -n \"\033]0;$Win_title\07\"; vi -c $linenum $filename; echo -n \"\033]0;$main_window_title\07\" ");
      #SystemExec ("echo \"\033]0;$Win_title\07\"; gvim -c $linenum $filename; echo \"\033]0;$main_window_title\07\" ");
    }
  }
  elsif (1) {
  }
  $input="a";
}

