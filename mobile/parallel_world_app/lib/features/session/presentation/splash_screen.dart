import 'package:flutter/material.dart';
import 'package:parallel_world_app/app/theme.dart';

class SplashScreen extends StatelessWidget {
  const SplashScreen({super.key});

  @override
  Widget build(BuildContext context) => const Scaffold(
    body: SafeArea(
      child: Center(
        child: Padding(
          padding: EdgeInsets.all(AppSpacing.large),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text('Parallel World', style: TextStyle(fontSize: 28)),
              SizedBox(height: AppSpacing.large),
              CircularProgressIndicator(semanticsLabel: 'Starting app'),
              SizedBox(height: AppSpacing.medium),
              Text('Preparing your private world…'),
            ],
          ),
        ),
      ),
    ),
  );
}
