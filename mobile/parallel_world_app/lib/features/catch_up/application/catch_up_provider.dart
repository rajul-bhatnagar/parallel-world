import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:parallel_world_app/core/errors/app_failure.dart';
import 'package:parallel_world_app/features/catch_up/application/catch_up_contracts.dart';
import 'package:parallel_world_app/features/catch_up/domain/catch_up_models.dart';

final catchUpGatewayProvider = Provider<CatchUpGateway>(
  (ref) => throw UnimplementedError('CatchUpGateway must be overridden.'),
);

class CatchUpView {
  const CatchUpView({this.result, this.summary, this.isOffline = false});

  final CatchUpResult? result;
  final CatchUpSummary? summary;
  final bool isOffline;
}

final catchUpProvider = FutureProvider.autoDispose.family<CatchUpView, String>((
  ref,
  worldId,
) async {
  try {
    final gateway = ref.read(catchUpGatewayProvider);
    final result = await gateway.process(worldId: worldId);
    return CatchUpView(
      result: result,
      summary: result.summary ?? await gateway.latest(worldId: worldId),
    );
  } on NetworkFailure {
    return const CatchUpView(isOffline: true);
  }
});
