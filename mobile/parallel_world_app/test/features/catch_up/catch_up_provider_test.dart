import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:parallel_world_app/features/catch_up/application/catch_up_contracts.dart';
import 'package:parallel_world_app/features/catch_up/application/catch_up_provider.dart';
import 'package:parallel_world_app/features/catch_up/domain/catch_up_models.dart';

import '../../support/fakes.dart';

void main() {
  test('not-due processing loads the latest authoritative summary', () async {
    const summary = CatchUpSummary(
      status: 'completed',
      text: 'Your world advanced.',
      items: [],
    );
    final gateway = _Gateway(summary);
    final container = ProviderContainer(
      overrides: [catchUpGatewayProvider.overrideWithValue(gateway)],
    );
    addTearDown(container.dispose);

    final view = await container.read(catchUpProvider(testWorld.id).future);

    expect(view.summary?.text, 'Your world advanced.');
    expect(gateway.processCalls, 1);
    expect(gateway.latestCalls, 1);
  });
}

class _Gateway implements CatchUpGateway {
  _Gateway(this.summary);

  final CatchUpSummary summary;
  int processCalls = 0;
  int latestCalls = 0;

  @override
  Future<CatchUpResult> process({required String worldId}) async {
    processCalls++;
    return const CatchUpResult(
      disposition: 'notdue',
      processedIntervals: 0,
      remainingIntervals: 0,
    );
  }

  @override
  Future<CatchUpSummary?> latest({required String worldId}) async {
    latestCalls++;
    return summary;
  }
}
