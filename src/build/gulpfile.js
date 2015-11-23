var gulp = require('gulp'),
    //debug = require('gulp-debug'),
    merge = require('merge-stream'),
    flatten = require('gulp-flatten'),
    shell = require('gulp-shell'),
    del = require('del'),
    msbuild = require('gulp-msbuild'),
    assemblyInfo = require('gulp-dotnet-assembly-info'),
    argv = require('yargs').argv,
    sequence = require('run-sequence');

var ver = '1.0.0',
    config = {
        srcdir: './../',
        projects: ['Requester', 'Requester.Validation'],
        build: {
            outdir: './artifacts/',
            version: ver,
            semver: ver + '-rc1',
            revision: argv && argv.buildrevision ? argv.buildrevision : '0',
            profile: argv && argv.buildprofile ? argv.buildprofile : 'Release'
        }
    };

gulp.task('default', function (cb) {
    sequence(
        ['clean', 'assemblyinfo'],
        'build',
        'integration-tests',
        'copy',
        cb);
});

gulp.task('ci', function (cb) {
    sequence(
        ['init-tools', 'clean', 'assemblyinfo', 'nuget-restore'],
        'build',
        'integration-tests',
        'copy',        
        'nuget-pack',
        cb);
});

gulp.task('init-tools', shell.task('nuget restore ./tools/packages.config -o ./tools/'));

gulp.task('nuget-restore', function () {
    return gulp.src(config.srcdir + '*.sln', { read: false })
        .pipe(shell('nuget restore <%= file.path %>'));
});

gulp.task('clean', function(cb) {
    del(config.build.outdir, cb);
});

gulp.task('assemblyinfo', function() {
    return gulp.src(config.srcdir + 'GlobalAssemblyInfo.cs')
        .pipe(assemblyInfo({
          version: config.build.version + '.' + config.build.revision,
          informationalVersion: config.build.semver
        }))
        .pipe(gulp.dest(config.srcdir));
});

gulp.task('build', function() {
    return gulp.src(config.srcdir + '*.sln')
        .pipe(msbuild({
            toolsVersion: 14.0,
            configuration: config.build.profile,
            targets: ['Clean', 'Rebuild'],
            errorOnFail: true,
            stdout: true,
            verbosity: 'minimal'
        }));
});

gulp.task('copy', function() {
    var tasks = config.projects.map(function (name) {
        return gulp.src(config.srcdir + 'projects/' + name + '/bin/' + config.build.profile + '/*.{dll,XML}')
            .pipe(flatten())
            .pipe(gulp.dest(config.build.outdir + '/' + name));
    });

    return merge(tasks);
});

gulp.task('integration-tests', function () {
    return gulp.src(config.srcdir + 'tests/**/bin/' + config.build.profile + '/*.IntegrationTests.dll')
        .pipe(shell('xunit.console.exe <%= file.path %> -noshadow', { cwd: './tools/xunit.runner.console.2.1.0/tools/' }));
});

gulp.task('nuget-pack', function () {
    return gulp.src('*.nuspec', { read: false })
        .pipe(shell('nuget pack <%= file.path %> -version ' + config.build.semver + ' -basepath ' + config.build.outdir + ' -o ' + config.build.outdir));
});
