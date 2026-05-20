import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../../core/api/api_client.dart';
import '../../../core/api/endpoints.dart';
import '../../../core/theme/app_theme.dart';
import '../providers/word_count_provider.dart';

class EditorScreen extends ConsumerStatefulWidget {
  const EditorScreen({super.key, required this.cadaverId});
  final String cadaverId;

  @override
  ConsumerState<EditorScreen> createState() => _EditorScreenState();
}

class _EditorScreenState extends ConsumerState<EditorScreen> {
  final _contentController = TextEditingController();
  String? _previousFragment;
  bool _loadingFragment = true;
  bool _submitting = false;

  @override
  void initState() {
    super.initState();
    _loadLastFragment();
  }

  Future<void> _loadLastFragment() async {
    try {
      final resp =
          await apiClient.get(Endpoints.lastFragment(widget.cadaverId));
      setState(() {
        _previousFragment = resp.statusCode == 204
            ? null
            : resp.data['content'] as String?;
        _loadingFragment = false;
      });
    } catch (_) {
      setState(() => _loadingFragment = false);
    }
  }

  Future<void> _submit() async {
    setState(() => _submitting = true);
    try {
      await apiClient.post(
        Endpoints.addFragment(widget.cadaverId),
        data: {'content': _contentController.text},
      );
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('¡Fragmento enviado!')),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context)
            .showSnackBar(SnackBar(content: Text('Error: $e')));
      }
    } finally {
      if (mounted) setState(() => _submitting = false);
    }
  }

  @override
  void dispose() {
    _contentController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final wordCount =
        ref.watch(wordCountProvider(_contentController.text));
    final overLimit = wordCount > 300;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.background,
        elevation: 0,
        leading: const BackButton(color: AppColors.textDark),
        title: Text(
          'Tu turno',
          style:
              GoogleFonts.playfairDisplay(color: AppColors.textDark),
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16),
            child: Center(
              child: Text(
                '$wordCount / 300',
                style: GoogleFonts.dmSans(
                  color: overLimit ? Colors.red : AppColors.textMuted,
                  fontSize: 13,
                ),
              ),
            ),
          ),
        ],
      ),
      body: _loadingFragment
          ? const Center(child: CircularProgressIndicator())
          : Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                children: [
                  SoftCard(
                    child: _previousFragment == null
                        ? Text(
                            'Eres el primero. Comienza la historia.',
                            style: GoogleFonts.playfairDisplay(
                                color: AppColors.textMuted,
                                fontSize: 15,
                                fontStyle: FontStyle.italic),
                          )
                        : Text(
                            '...${_previousFragment!}',
                            style: GoogleFonts.playfairDisplay(
                                color: AppColors.textDark,
                                fontSize: 15,
                                height: 1.6),
                          ),
                  ),
                  const SizedBox(height: 16),
                  Expanded(
                    child: TextField(
                      controller: _contentController,
                      onChanged: (_) => setState(() {}),
                      maxLines: null,
                      expands: true,
                      textAlignVertical: TextAlignVertical.top,
                      style: GoogleFonts.playfairDisplay(
                          color: AppColors.textDark,
                          fontSize: 15,
                          height: 1.7),
                      decoration: InputDecoration(
                        hintText: 'Continúa la historia...',
                        hintStyle: GoogleFonts.playfairDisplay(
                            color: AppColors.textMuted),
                        alignLabelWithHint: true,
                      ),
                    ),
                  ),
                  const SizedBox(height: 16),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: (_submitting ||
                              overLimit ||
                              wordCount == 0)
                          ? null
                          : _submit,
                      child: _submitting
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(
                                  strokeWidth: 2,
                                  color: Colors.white))
                          : const Text('Enviar fragmento'),
                    ),
                  ),
                ],
              ),
            ),
    );
  }
}
