import 'package:flutter/material.dart';

void main() {
  runApp(const ParallelWorldApp());
}

class ParallelWorldApp extends StatelessWidget {
  const ParallelWorldApp({super.key});

  @override
  Widget build(BuildContext context) {
    return const MaterialApp(
      debugShowCheckedModeBanner: false,
      home: Scaffold(body: Center(child: Text('Parallel World'))),
    );
  }
}
