import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/main.dart';

void main() {
  testWidgets('ApplicationShell_Builds_ShowsProductName', (tester) async {
    await tester.pumpWidget(const ParallelWorldApp());

    expect(find.text('Parallel World'), findsOneWidget);
  });
}
