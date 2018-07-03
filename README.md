# TextureBundler

Given a directory path, will bundle all images it finds within into a single png file, with an attached text file giving filename and x, y, w, h coordinates.

The bundling is done into a rough square, and each image is given the same space. Accordingly, this project works best if all images are the same size, or close to.

Final result will be a output.png and output.csv file in the run directory (not the target directory!).

Coded in F#, for use in my game dev projects.