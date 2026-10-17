import 'package:flutter/material.dart';
import 'package:parallel_world_app/app/app.dart';
import 'package:parallel_world_app/app/dependencies.dart';
import 'package:parallel_world_app/core/config/app_config.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  try {
    final config = AppConfig.fromEnvironment();
    runApp(buildAppScope(config: config, child: const ParallelWorldApp()));
  } on AppConfigException catch (error) {
    runApp(ConfigurationFailureApp(message: error.message));
  }
}
