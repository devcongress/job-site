'use strict';

var gulp = require('gulp'),
    sass = require('gulp-sass');

gulp.task("sass", function () {
    gulp.src("sass/app.sass")
        .pipe(sass())
        .pipe(gulp.dest("css"));
});

gulp.task('default', ['sass']);
