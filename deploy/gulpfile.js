var gulp = require('gulp'),
    //debug = require('gulp-debug'),
    flatten = require('gulp-flatten'),
    shell = require('gulp-shell'),
    del = require('del'),
    msbuild = require('gulp-msbuild'),
    assemblyInfo = require('gulp-dotnet-assembly-info'),
    argv = require('yargs').argv,
    sequence = require('run-sequence');

var config = {
  slnname: 'Requester',
  src: './../src/',
  build: {
    outdir: './build/',
    version: '0.1.1',
    revision: argv.buildrevision || '0',
    profile: argv.buildprofile || 'Release'
  }
};

gulp.task('default', function (cb) {
  sequence(
    ['clean', 'assemblyinfo'],
    'build', 'copy', 'integration-tests', cb);
});

gulp.task('ci', function (cb) {
  sequence(
    ['init-tools', 'clean', 'assemblyinfo', 'nuget-restore'],
    'build', 'copy', 'integration-tests', 'nuget-pack', cb);
});

gulp.task('init-tools', shell.task([
  'nuget restore ./tools/packages.config -o ./tools/']
));

gulp.task('nuget-restore', function () {
  return gulp.src(config.src + '*.sln', { read: false })
    .pipe(shell('nuget restore <%= file.path %>'));
});

gulp.task('clean', function(cb) {
  del(config.build.outdir, cb);
});

gulp.task('assemblyinfo', function() {
  return gulp
    .src(config.src + 'SharedAssemblyInfo.cs')
    .pipe(assemblyInfo({
      version: config.build.version,
      fileVersion: config.build.version + '.' + config.build.revision,
    }))
    .pipe(gulp.dest(config.src));
});

gulp.task('build', function() {
  return gulp.src(config.src + '*.sln')
    .pipe(msbuild({
      toolsVersion: 12.0,
      configuration: config.build.profile,
      targets: ['Clean', 'Build'],
      errorOnFail: true,
      stdout: true,
      verbosity: 'minimal'
    }));
});

gulp.task('copy', function() {
  return gulp.src(config.src + 'projects/**/bin/' + config.build.profile + '/*.{dll,XML}')
    .pipe(flatten())
    .pipe(gulp.dest(config.build.outdir));
});

gulp.task('integration-tests', function () {
  return gulp.src(config.src + 'tests/**/bin/' + config.build.profile + '/*.IntegrationTests.dll')
    .pipe(shell('xunit.console.exe <%= file.path %> -noshadow', { cwd: './tools/xunit.runner.console.2.0.0/tools/' }));
});

gulp.task('nuget-pack', shell.task([
  'nuget pack ./' + config.slnname + '.nuspec -version ' + config.build.version + ' -basepath ' + config.build.outdir + ' -o ' + config.build.outdir]
));